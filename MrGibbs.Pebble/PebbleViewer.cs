using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MrGibbs.Contracts;
using MrGibbs.Contracts.Infrastructure;
using MrGibbs.Models;

using PebbleSharp.Core;
using PebbleSharp.Core.NonPortable.AppMessage;
using PebbleSharp.Core.Bundles;
using PebbleSharp.Core.Responses;

namespace MrGibbs.Pebble
{
    internal class LineStateMap
    {
        //func that given the state, returns a string that will be displayed on the dashboard
        public Func<State, string> Get { get; private set; }
        //the label that will appear on the dashboard for this map
        public string Caption { get; private set; }
        //an option action that cwill be taken when the user pushes the button next to the line on the dashboard (up or down only)
        public Action Action { get; private set; }

        public LineStateMap(Func<State, string> f, string caption)
        {
            Get = f;
            Caption = caption;
            Action = null;
        }

        public LineStateMap(Func<State, string> f, string caption, Action action)
        {
            Get = f;
            Caption = caption;
            Action = action;
        }
    }

    public class PebbleViewer:IViewer
    {
        private enum UICommand:byte{Button=0,Dash=1,Course=2,Mark=3,NewRace=4,Calibrate=5,Restart=6,Reboot=7,Shutdown=8}
        private enum Button : byte { Up = 0, Select = 1, Down = 2 }

        private ILogger _logger;
        private PebblePlugin _plugin;
        private PebbleSharp.Core.Pebble _pebble;
        private byte _transactionId;
        private UUID _uuid;
        
        
        private int _lineCount;//number of rows shown on the pebble UI
        private volatile IList<int> _lineValueIndexes;//index of which map we're currently showing on each line
        private static IList<LineStateMap> _lineStateMaps;

        private static Dictionary<UICommand, Action<ApplicationMessageResponse>> _commandMaps;

        private Action<Action<ISystemController, IRaceController>> _queueCommand;

        private TimeSpan _sendTimeout=new TimeSpan(0,0,0,5);
        private Task _lastSend;
        private DateTime? _lastSendAt;

        public PebbleViewer(ILogger logger, PebblePlugin plugin, PebbleSharp.Core.Pebble pebble, AppBundle bundle, Action<Action<ISystemController, IRaceController>> queueCommand)
        {
            _queueCommand = queueCommand;
            _plugin = plugin;
            _logger = logger;
            _pebble = pebble;
            _pebble.ConnectAsync().Wait();
			_logger.Info ("Connected to pebble " + _pebble.PebbleID);
            _transactionId = 255;

            _uuid = new UUID(bundle.AppInfo.UUID);

            var getAppBank = _pebble.GetAppbankContentsAsync();
            getAppBank.Wait();
            var bank = getAppBank.Result;

            if (bank.AppUUIDs.Contains(_uuid))
            {
                _logger.Info("Pebble "+_pebble.PebbleID+" already has app installed, removing...");
                var index = bank.AppUUIDs.IndexOf(_uuid);
                var remove = _pebble.RemoveAppAsync(bank.AppBank.Apps[index]);
                remove.Wait();
            }

            var progress = new Progress<ProgressValue>(pv => _logger.Debug("Installing app on pebble "+pebble.PebbleID+", "+pv.ProgressPercentage+"% complete. "+pv.Message));
            var install = _pebble.InstallAppAsync(bundle,progress);
            install.Wait();
            _logger.Info("Installed app on pebble " + pebble.PebbleID);


            var launch = _pebble.LaunchApp( _uuid);
            launch.Wait();
            _logger.Info("Launched app on pebble " + pebble.PebbleID);

            _pebble.RegisterCallback<ApplicationMessageResponse>(Receive);

            InitializeViewer();
        }

        private void InitializeViewer()
        {
            _lineCount = 3;
            if (_lineStateMaps == null)
            {
                /* These indexes MUST match the order used on the pebble otherwise things will get weird
                    Speed
                    VMG
                    VMC
                    COG
                    Heading (mag)
                    Heading (True)
                    Heel
                    Wind Speed (Apparant)
                    Wind Speed (True)
                    Wind Direction (Apparant)
                    Wind Direction (True)
                    Nominal Speed
                    % Nominal Speed
                    Top Speed
                    Countdown
                    Distance to Mark*/
                _lineStateMaps = new List<LineStateMap>();
                _lineStateMaps.Add(new LineStateMap(s => s.SpeedInKnots.HasValue ? string.Format("{0:0.0}", s.SpeedInKnots.Value) : "", "Speed (kn)"));
                _lineStateMaps.Add(new LineStateMap(s => s.VelocityMadeGood.HasValue ? string.Format("{0:0.0}", s.VelocityMadeGood.Value) : "", "VMG (kn)"));
                _lineStateMaps.Add(new LineStateMap(s => s.VelocityMadeGoodOnCourse.HasValue ? string.Format("{0:0.0}", s.VelocityMadeGoodOnCourse.Value) : "", "VMC (kn)"));
                _lineStateMaps.Add(new LineStateMap(s=>s.CourseOverGroundByLocation.HasValue ? string.Format("{0:0.0}",s.CourseOverGroundByLocation.Value):"","Course Over Ground"));
                _lineStateMaps.Add(new LineStateMap(s => s.MagneticHeading.HasValue ? string.Format("{0:0.0}", s.MagneticHeading.Value) : "", "Heading (Mag)"));
                _lineStateMaps.Add(new LineStateMap(s => s.MagneticHeadingWithVariation.HasValue ? string.Format("{0:0.0}", s.MagneticHeadingWithVariation.Value) : "", "Heading (True)"));
                _lineStateMaps.Add(new LineStateMap(s => s.Heel.HasValue ? string.Format("{0:0.0}", s.Heel.Value) : "", "Heel"));
                //_lineStateMaps.Add(new LineStateMap(s => s.Pitch.HasValue ? string.Format("{0:0.0}", s.Pitch.Value) : "", "Pitch"));
                _lineStateMaps.Add(new LineStateMap(s => "", "Wind Speed (Apparant)"));
                _lineStateMaps.Add(new LineStateMap(s => "", "Wind Speed (True)"));
                _lineStateMaps.Add(new LineStateMap(s => "", "Wind Direction (Aparant)"));
                _lineStateMaps.Add(new LineStateMap(s => "", "Wind Direction (True)"));
                _lineStateMaps.Add(new LineStateMap(s => "", "Nominal Speed"));
                _lineStateMaps.Add(new LineStateMap(s => "", "% Nominal Speed"));
                _lineStateMaps.Add(new LineStateMap(s => s.MaximumSpeedInKnots.HasValue ? string.Format("{0:0.0}", s.MaximumSpeedInKnots.Value) : "", "Top Speed (kn)"));
                _lineStateMaps.Add(new LineStateMap(s => s.Countdown.HasValue ? s.Countdown.Value.Minutes + ":" + s.Countdown.Value.Seconds.ToString("00") : "", "Countdown",()=>_queueCommand((s,r)=>r.CountdownAction())));
                _lineStateMaps.Add(new LineStateMap(s => s.DistanceToTargetMarkInYards.HasValue ? string.Format("{0}{1:0}",s.TargetMark!=null ? s.TargetMark.Abbreviation : "",s.DistanceToTargetMarkInYards.Value) :"?", "Distance to Mark (yds)"));
                _lineStateMaps.Add(new LineStateMap(s => s.Pitch.HasValue ? string.Format("{0:0.0}", s.Pitch.Value) : "", "Pitch"));
            }

            _lineValueIndexes = new List<int>();
            _lineValueIndexes.Add(0);//speed
            _lineValueIndexes.Add(3);//cog
            _lineValueIndexes.Add(14);//countdown

            if(_commandMaps==null)
            {
                _commandMaps = new Dictionary<UICommand, Action<ApplicationMessageResponse>>();
                _commandMaps.Add(UICommand.Dash, ProcessDashCommand);
                _commandMaps.Add(UICommand.Button, ProcessButtonCommand);
                _commandMaps.Add(UICommand.Calibrate, m=>_queueCommand((s,r)=>s.Calibrate()));
                _commandMaps.Add(UICommand.Restart, m => _queueCommand((s, r) => s.Restart()));
                _commandMaps.Add(UICommand.Reboot, m => _queueCommand((s, r) => s.Reboot()));
                _commandMaps.Add(UICommand.Shutdown, m => _queueCommand((s, r) => s.Shutdown()));
                _commandMaps.Add(UICommand.Mark, ProcessMarkCommand);
            }
        }

        private void Receive(ApplicationMessageResponse response)
        {
            if (response.Dictionary != null)
            {
                var commandTuple = response.Dictionary.Values.SingleOrDefault(x => x.Key == 0);
                if(commandTuple!=null && commandTuple is AppMessageUInt8)
                {
                    UICommand command = (UICommand)((AppMessageUInt8)commandTuple).Value;
                    
                    if(_commandMaps.ContainsKey(command))
                    {
                        _commandMaps[command](response);
                    }
                    else
                    {
                        _logger.Info("Received Command " + command.ToString() + " from pebble " + _pebble.PebbleID+", but there is no map specified");
                    }
                }
            }
        }

        private void ProcessButtonCommand(ApplicationMessageResponse response)
        {
            var lineTuple = response.Dictionary.Values.SingleOrDefault(x=>x.Key==1);
            if(lineTuple!=null && lineTuple is AppMessageUInt8)
            {
                var line = ((AppMessageUInt8)lineTuple).Value;
                var action = _lineStateMaps[_lineValueIndexes[line]].Action;
                if(action!=null)
                {
                    _logger.Info("Received button press for line " + line + ", executing action for " + _lineStateMaps[_lineValueIndexes[line]].Caption);
                    action();
                }
                else
                {
                    _logger.Info("Received button press for line " + line+", but there is no action defined for "+_lineStateMaps[_lineValueIndexes[line]].Caption);
                }
            }
        }

        private void ProcessDashCommand(ApplicationMessageResponse response)
        {
            //which line are we changing?
            var line = ((AppMessageUInt8)response.Dictionary.Values.SingleOrDefault(x => x.Key == 1)).Value;
            //change it to which map?
            var map = ((AppMessageUInt8)response.Dictionary.Values.SingleOrDefault(x => x.Key == 2)).Value;

            _logger.Info("Pebble "+_pebble.PebbleID+" Has requested Dashboard Row "+line+" to show "+_lineStateMaps[map].Caption);

            lock (_lineValueIndexes)
            {
                _lineValueIndexes[(int) line] = (int) map;
            }
        }

        private void ProcessMarkCommand(ApplicationMessageResponse response)
        {
            var mark = (MarkType)((AppMessageUInt8)response.Dictionary.Values.SingleOrDefault(x => x.Key == 1)).Value;

            var bearingTuple = response.Dictionary.Values.SingleOrDefault(x=>x.Key==2);
            if (bearingTuple!=null)
            {
                //bearing
                int pebbleBearing = ((AppMessageInt32)bearingTuple).Value;
                //convert to double
                double bearingToNorth = ((double)pebbleBearing/65536.0)*360.0;

                //pebble reports bearing to NORTH, we need to figure out what we're pointed at
                double bearingToMark = 360 - bearingToNorth;

                

                _queueCommand((s, r) => r.SetMarkBearing(mark, bearingToMark, true));
            }
            else
            {
                //location
                _queueCommand((s, r) => r.SetMarkLocation(mark));
            
            }
        }

        public void Update(State state)
        {

            //don't send anything until the last send has completed or errored
            if (_lastSend == null || _lastSend.IsCanceled || _lastSend.IsCompleted || _lastSend.IsFaulted 
                //or if it has exceeded the send timeout
                || !_lastSendAt.HasValue || state.SystemTime - _lastSendAt.Value > _sendTimeout)
            {
#if WINDOWS
                //give us some smooth numbers to look at when we're testing on 'doze
                if(!state.MagneticHeading.HasValue)
                {
                    state.MagneticHeading = 0;
                }
                else
                {
                    state.MagneticHeading += 1;
                    state.MagneticHeading = state.MagneticHeading % 360;
                }
#endif

                _transactionId--;
                AppMessageDictionary message = new AppMessageDictionary();
                message.ApplicationId = _uuid;
                message.TransactionId = _transactionId;
                message.Command = (byte) Command.Push;

                string captions = "";

                for (int i = 0; i < _lineCount; i++)
                {
                    LineStateMap map = null;
                    lock (_lineValueIndexes)
                    {
                        map = _lineStateMaps[_lineValueIndexes[i]];
                    }
                    message.Values.Add(new AppMessageString() {Key = (uint) message.Values.Count, Value = map.Caption});
                    captions = captions + map.Caption + ",";
                    message.Values.Add(new AppMessageString()
                    {
                        Key = (uint) message.Values.Count,
                        Value = map.Get(state)
                    });
                }

                if (state.Message != null)
                {
                    message.Values.Add(new AppMessageString()
                    {
                        Key = (uint) message.Values.Count,
                        Value = state.Message.Text
                    });
                }

                _lastSend = _pebble.SendApplicationMessage(message);
                _lastSendAt = state.SystemTime;
                _logger.Debug("Sent state to pebble " + _pebble.PebbleID+" ("+captions+")");
            }
            else
            {
                //_logger.Debug("Skipped send to pebble, previous send has not completed yet");
            }
        }

        public IPlugin Plugin
        {
            get { return _plugin; }
        }

        public void Dispose()
        {
            _pebble.Disconnect();

        }
    }
}
