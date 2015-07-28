using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PovertySail.Contracts;
using PovertySail.Contracts.Infrastructure;
using PovertySail.Models;

using PebbleSharp.Core;
using PebbleSharp.Core.NonPortable.AppMessage;
using PebbleSharp.Core.Bundles;
using PebbleSharp.Core.Responses;

namespace PovertySail.Pebble
{
    internal class LineStateMap
    {
        //func that given the state, returns a string that will be displayed on the dashboard
        public Func<State, string> Get { get; private set; }
        //the label that will appear on the dashboard for this map
        public string Caption { get; private set; }
        //an option action that cwill be taken when the user pushes the button next to the line on the dashboard (up or down only)
        public Action<IRaceController> Action { get; private set; }

        public LineStateMap(Func<State, string> f, string caption)
        {
            Get = f;
            Caption = caption;
            Action = null;
        }

        public LineStateMap(Func<State, string> f, string caption, Action<IRaceController> action)
        {
            Get = f;
            Caption = caption;
            Action = action;
        }
    }

    public class PebbleViewer:IViewer
    {
        private enum UICommand:byte{Button=0,Dash=1,Course=2,Mark=3,NewRace=4,Calibrate=5,Restart=6,Reboot=7}
        private enum Button : byte { Up = 0, Select = 1, Down = 2 }

        private ILogger _logger;
        private PebblePlugin _plugin;
        private PebbleSharp.Core.Pebble _pebble;
        private byte _transactionId;
        private UUID _uuid;
        
        private int _lineCount;//number of rows shown on the pebble UI
        private IList<int> _lineValueIndexes;//index of which map we're currently showing on each line
        private static IList<LineStateMap> _lineStateMaps;

        private static Dictionary<UICommand, Action<ApplicationMessageResponse,ISystemController ,IRaceController>> _commandMaps;

        private IRaceController _raceController;
        private ISystemController _systemController;

        private Task _lastSend;

        public PebbleViewer(ILogger logger, PebblePlugin plugin, PebbleSharp.Core.Pebble pebble, AppBundle bundle, ISystemController systemController, IRaceController raceControllerr)
        {
            _raceController = raceControllerr;
            _systemController = systemController;
            _plugin = plugin;
            _logger = logger;
            _pebble = pebble;
            _pebble.ConnectAsync().Wait();
			_logger.Info ("Connected to pebble " + _pebble.PebbleID);
            _transactionId = 255;

            _uuid = new UUID(bundle.AppInfo.UUID);
            
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
                    Heading
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
                _lineStateMaps.Add(new LineStateMap(s => "", "VMG"));
                _lineStateMaps.Add(new LineStateMap(s => "", "VMC"));
                _lineStateMaps.Add(new LineStateMap(s=>s.CourseOverGroundByLocation.HasValue ? string.Format("{0:0.0}",s.CourseOverGroundByLocation.Value):"","Course Over Ground"));
                _lineStateMaps.Add(new LineStateMap(s => s.MagneticHeading.HasValue ? string.Format("{0:0.0}", s.MagneticHeading.Value) : "", "Magnetic Heading"));
                _lineStateMaps.Add(new LineStateMap(s => s.Heel.HasValue ? string.Format("{0:0.0}", s.Heel.Value) : "", "Heel"));
                _lineStateMaps.Add(new LineStateMap(s => "", "Wind Speed (Apparant)"));
                _lineStateMaps.Add(new LineStateMap(s => "", "Wind Speed (True)"));
                _lineStateMaps.Add(new LineStateMap(s => "", "Wind Direction (Aparant)"));
                _lineStateMaps.Add(new LineStateMap(s => "", "Wind Direction (True)"));
                _lineStateMaps.Add(new LineStateMap(s => "", "Nominal Speed"));
                _lineStateMaps.Add(new LineStateMap(s => "", "% Nominal Speed"));
                _lineStateMaps.Add(new LineStateMap(s => "", "Top Speed"));
                _lineStateMaps.Add(new LineStateMap(s => s.Countdown.HasValue ? s.Countdown.Value.Minutes + ":" + s.Countdown.Value.Seconds.ToString("00") : "", "Countdown",c=>c.CountdownAction()));
                _lineStateMaps.Add(new LineStateMap(s => s.DistanceToTargetMarkInYards.HasValue ? string.Format("{0:0}",s.DistanceToTargetMarkInYards.Value) :"?", "Distance to Mark (yds)"));
            }

            _lineValueIndexes = new List<int>();
            _lineValueIndexes.Add(0);//speed
            _lineValueIndexes.Add(3);//heading
            _lineValueIndexes.Add(13);//countdown

            if(_commandMaps==null)
            {
                _commandMaps = new Dictionary<UICommand, Action<ApplicationMessageResponse,ISystemController ,IRaceController>>();
                _commandMaps.Add(UICommand.Dash, ProcessDashCommand);
                _commandMaps.Add(UICommand.Button, ProcessButtonCommand);
                _commandMaps.Add(UICommand.Calibrate, (m, s, r) => s.Calibrate());
                _commandMaps.Add(UICommand.Restart, (m, s, r) => s.Restart());
                _commandMaps.Add(UICommand.Reboot, (m, s, r) => s.Reboot());
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
                    _logger.Info("Received Command " + command.ToString() + "from pebble " + _pebble.PebbleID);

                    if(_commandMaps.ContainsKey(command))
                    {
                        _commandMaps[command](response,_systemController,_raceController);
                    }
                    else
                    {
                        _logger.Info("No Command map for "+command);
                    }
                }
            }
        }

        private void ProcessButtonCommand(ApplicationMessageResponse response,ISystemController systemController,IRaceController controller)
        {
            var lineTuple = response.Dictionary.Values.SingleOrDefault(x=>x.Key==1);
            if(lineTuple!=null && lineTuple is AppMessageUInt8)
            {
                var line = ((AppMessageUInt8)lineTuple).Value;
                var action = _lineStateMaps[_lineValueIndexes[line]].Action;
                if(action!=null)
                {
                    _logger.Info("Received button press for line " + line + ", executing action for " + _lineStateMaps[_lineValueIndexes[line]].Caption);
                    action(controller);
                }
                else
                {
                    _logger.Info("Received button press for line " + line+", but there is no action defined for "+_lineStateMaps[_lineValueIndexes[line]].Caption);
                }
            }
        }

        private void ProcessDashCommand(ApplicationMessageResponse response, ISystemController systemController, IRaceController controller)
        {
            //which line are we changing?
            var line = ((AppMessageUInt8)response.Dictionary.Values.SingleOrDefault(x => x.Key == 1)).Value;
            //change it to which map?
            var map = ((AppMessageUInt8)response.Dictionary.Values.SingleOrDefault(x => x.Key == 2)).Value;
            _lineValueIndexes[(int)line] = (int)map;
        }

        private void ProcessMarkCommand(ApplicationMessageResponse response, ISystemController systemController, IRaceController controller)
        {
            var mark = (MarkType)((AppMessageUInt8)response.Dictionary.Values.SingleOrDefault(x => x.Key == 1)).Value;

            var bearingTuple = response.Dictionary.Values.SingleOrDefault(x=>x.Key==2);
            if (bearingTuple!=null)
            {
                //bearing
                int pebbleBearing = ((AppMessageInt32)bearingTuple).Value;
                //convert to double
                double bearing=0;
                //TODO convert from pebble triangle to double degrees
                controller.SetMarkBearing(mark, bearing,true);
            }
            else
            {
                //location
                controller.SetMarkLocation(mark);
            }
        }

        public void Update(State state)
        {
            //don't send anything until the last send has completed
            if (_lastSend == null || _lastSend.IsCanceled || _lastSend.IsCompleted || _lastSend.IsFaulted)
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

                for (int i = 0; i < _lineCount; i++)
                {
                    var map = _lineStateMaps[_lineValueIndexes[i]];
                    message.Values.Add(new AppMessageString() {Key = (uint) message.Values.Count, Value = map.Caption});
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
                _logger.Debug("Sent state to pebble " + _pebble.PebbleID);
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
