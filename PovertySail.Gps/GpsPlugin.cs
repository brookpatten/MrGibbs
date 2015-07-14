using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PovertySail.Contracts;
using PovertySail.Contracts.Infrastructure;

namespace PovertySail.Gps
{
    public class GpsPlugin:IPlugin
    {
        private ILogger _logger;
        private bool _initialized = false;

        public GpsPlugin(ILogger logger)
        {
            _logger = logger;
        }

        public void Initialize(PluginConfiguration configuration)
        {
            _initialized = false;
            configuration.Sensors.Add(new GpsSensor(_logger,this));
            _initialized = true;
        }

        public bool Initialized
        {
            get { return _initialized; }
        }
    }
}
