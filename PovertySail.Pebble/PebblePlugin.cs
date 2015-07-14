using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PovertySail.Contracts;
using PovertySail.Contracts.Infrastructure;

namespace PovertySail.Pebble
{
    public class PebblePlugin:IPlugin
    {
        private ILogger _logger;
        private bool _initialized = false;

        
        public PebblePlugin(ILogger logger)
        {
            _logger = logger;
        }

        public void Initialize(PluginConfiguration configuration)
        {
            //scan for pebbles
            //add a viewer for each pebble
        }

        public bool Initialized
        {
            get { return _initialized; }
        }
    }
}
