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
        public Func<State, string> Get { get; private set; }
        public string Caption { get; private set; }

        public LineStateMap(Func<State, string> f, string caption)
        {
            Get = f;
            Caption = caption;
        }
    }

    public class PebbleViewer:IViewer
    {
        private ILogger _logger;
        private PebblePlugin _plugin;
        private PebbleSharp.Core.Pebble _pebble;
        
        private byte _transactionId;
        
        private UUID _uuid;
        //private Dictionary<string, int> _keys;

        private int _lineCount;
        private IList<int> _lineValueIndexes = new List<int>();

        private static IList<LineStateMap> _lineStateMaps;

        private Task _lastSend;

        public PebbleViewer(ILogger logger, PebblePlugin plugin, PebbleSharp.Core.Pebble pebble, AppBundle bundle)
        {
            _plugin = plugin;
            _logger = logger;
            _pebble = pebble;
            _pebble.ConnectAsync().Wait();
			_logger.Info ("Connected to pebble " + _pebble.PebbleID);
            _transactionId = 255;

            _uuid = new UUID(bundle.AppInfo.UUID);
            //keys = bundle.AppInfo.AppKeys;

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
                _lineStateMaps = new List<LineStateMap>();
                _lineStateMaps.Add(new LineStateMap(s=>s.CourseOverGroundByLocation.HasValue ? string.Format("{0:0.0}",s.CourseOverGroundByLocation.Value):"","Course Over Ground"));
                _lineStateMaps.Add(new LineStateMap(s => s.SpeedInKnots.HasValue ? string.Format("{0:0.0}", s.SpeedInKnots.Value) : "", "Speed (kn)"));
                _lineStateMaps.Add(new LineStateMap(s => s.Countdown.HasValue ? s.Countdown.Value.Minutes + ":" + s.Countdown.Value.Seconds.ToString("00") : "", "Countdown"));
                _lineStateMaps.Add(new LineStateMap(s => s.MagneticHeading.HasValue ? string.Format("{0:0.0}", s.MagneticHeading.Value) : "", "Magnetic Heading"));
                _lineStateMaps.Add(new LineStateMap(s => s.MagneticCourseMadeGood.HasValue ? string.Format("{0:0.0}", s.MagneticCourseMadeGood.Value) : "", "Magnetic Course Made Good"));
                _lineStateMaps.Add(new LineStateMap(s => s.TrueCourseMadeGood.HasValue ? string.Format("{0:0.0}", s.TrueCourseMadeGood.Value) : "", "True Course Made Good"));
                _lineStateMaps.Add(new LineStateMap(s => s.Heel.HasValue ? string.Format("{0:0.0}", s.Heel.Value) : "", "Heel"));
            }

            for (int i = 0; i < _lineCount && i<_lineStateMaps.Count; i++)
            {
                _lineValueIndexes.Add(i);
            }
        }

        private void Receive(ApplicationMessageResponse response)
        {
            if (response.Dictionary != null)
            {
                var command = (AppMessageString)response.Dictionary.Values.SingleOrDefault(x => x.Key == 0);
                if (command != null)
                {
                    var button = command.Value;

                    _logger.Info("Received Command " + button + "from pebble " + _pebble.PebbleID);

                    if (button == "up" && OnHeadingButton != null)
                    {
                        _lineValueIndexes[0] = (_lineValueIndexes[0]+1) % _lineStateMaps.Count;
                        OnHeadingButton(this, new EventArgs());
                    }
                    else if (button == "down" && OnWatchButton != null)
                    {
                        _lineValueIndexes[2] = (_lineValueIndexes[2] + 1) % _lineStateMaps.Count;
                        OnWatchButton(this, new EventArgs());
                    }
                    else if (button == "select" && OnSpeedButton != null)
                    {
                        _lineValueIndexes[1] = (_lineValueIndexes[1] + 1) % _lineStateMaps.Count;
                        OnSpeedButton(this, new EventArgs());
                    }
                }
            }
        }

        public void Update(State state)
        {
            //don't send anything until the last send has completed
            if (_lastSend == null || _lastSend.IsCanceled || _lastSend.IsCompleted || _lastSend.IsFaulted)
            {
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
        }

        public event EventHandler OnWatchButton;
        
        public event EventHandler OnHeadingButton;
        
        public event EventHandler OnSpeedButton;

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
