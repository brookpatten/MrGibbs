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
    public class PebbleViewer:IViewer
    {
        private ILogger _logger;
        private PebblePlugin _plugin;
        private PebbleSharp.Core.Pebble _pebble;
        
        private byte _transactionId;
        
        private UUID _uuid;
        //private Dictionary<string, int> _keys;

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
                        OnHeadingButton(this, new EventArgs());
                    }
                    else if (button == "down" && OnWatchButton != null)
                    {
                        OnWatchButton(this, new EventArgs());
                    }
                    else if (button == "select" && OnSpeedButton != null)
                    {
                        OnSpeedButton(this, new EventArgs());
                    }
                }
            }
        }

        public void Update(State state)
        {
            _transactionId--;
            AppMessageDictionary message = new AppMessageDictionary();
            message.ApplicationId = _uuid;
            message.TransactionId = _transactionId;
            message.Command = (byte)Command.Push;

            message.Values.Add(new AppMessageString() { Key = 0,Value = "Course over ground" });
            message.Values.Add(new AppMessageString() { Key = 1, Value = string.Format("{0:0.0}°", state.CourseOverGround) });
            message.Values.Add(new AppMessageString() { Key = 2, Value = "Speed (kn)" });
            message.Values.Add(new AppMessageString() { Key = 3, Value = string.Format("{0:0.0}", state.SpeedInKnots) });
            message.Values.Add(new AppMessageString() { Key = 4, Value = state.Countdown.HasValue ? "Countdown":"" });
            string countdown = state.Countdown.HasValue ? state.Countdown.Value.Minutes + ":" + state.Countdown.Value.Seconds.ToString("00") : "";
            message.Values.Add(new AppMessageString() { Key = 5, Value = countdown });
            if (state.Message!=null)
            {
                message.Values.Add(new AppMessageString() { Key = 6, Value = state.Message.Text });
            }

            var t = _pebble.SendApplicationMessage(message);
			_logger.Debug ("Sent state to pebble " + _pebble.PebbleID);
            //t.Start();
            //t.Wait();
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
