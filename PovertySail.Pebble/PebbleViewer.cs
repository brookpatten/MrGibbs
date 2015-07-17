using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PovertySail.Contracts;
using PovertySail.Contracts.Infrastructure;
using PovertySail.Models;

namespace PovertySail.Pebble
{
    public class PebbleViewer:IViewer
    {
        private ILogger _logger;
        private PebblePlugin _plugin;

        public PebbleViewer(ILogger logger, PebblePlugin plugin)
        {
            _plugin = plugin;
            _logger = logger;
        }

        public void Update(State state)
        {
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
            
        }
    }
}
