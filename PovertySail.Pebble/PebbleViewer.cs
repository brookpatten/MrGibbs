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

namespace PovertySail.Pebble
{
    public class PebbleViewer:IViewer
    {
        private ILogger _logger;
        private PebblePlugin _plugin;
        private PebbleSharp.Core.Pebble _pebble;
        private byte _transactionId;

        public PebbleViewer(ILogger logger, PebblePlugin plugin, PebbleSharp.Core.Pebble pebble)
        {
            _plugin = plugin;
            _logger = logger;
            _pebble = pebble;
            _pebble.ConnectAsync().Wait();
			_logger.Info ("Connected to pebble " + _pebble.PebbleID);
            _transactionId = 255;
        }

        public void Update(State state)
        {
            _transactionId--;
            UUID uuid = new UUID("22a27b9a-0b07-47af-ad87-b2c29305bab6");
            AppMessageDictionary message = new AppMessageDictionary();
            message.ApplicationId = uuid;
            message.TransactionId = _transactionId;

            message.Values.Add(new AppMessageString() { Value = "Course over ground" });
            message.Values.Add(new AppMessageString() { Value = string.Format("{0:0.0}°", state.CourseOverGround) });
            message.Values.Add(new AppMessageString() { Value = "Speed" });
            message.Values.Add(new AppMessageString() { Value = string.Format("{0:0.0}", state.Speed) });
            message.Values.Add(new AppMessageString() { Value = "" });
            message.Values.Add(new AppMessageString() { Value = ":)" });


            var t = _pebble.SendApplicationMessage(message);
			_logger.Debug ("Sent state to pebble " + _pebble.PebbleID);
            //t.Start();
            //t.Wait();
        }

        public event EventHandler OnStartCountdown;

        public event EventHandler OnSyncCountdown;

        public event EventHandler OnStopCountdown;

        public event EventHandler OnSetMark;

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
