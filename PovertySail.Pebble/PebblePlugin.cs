using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PebbleSharp.Net45;
using PovertySail.Contracts;
using PovertySail.Contracts.Infrastructure;

namespace PovertySail.Pebble
{
    public class PebblePlugin:IPlugin
    {
        private ILogger _logger;
        private bool _initialized = false;
        private IList<IPluginComponent> _components; 

        
        public PebblePlugin(ILogger logger)
        {
            _logger = logger;
            _components = new List<IPluginComponent>();
        }

        public void Initialize(PluginConfiguration configuration)
        {
            //scan for pebbles
            var pebbles = PebbleNet45.DetectPebbles();
            //add a viewer for each pebble
            foreach (var pebble in pebbles)
            {
                try
                {
                    var viewer = new PebbleViewer(_logger, this, pebble);
                    _components.Add(viewer);
                    configuration.DashboardViewers.Add(viewer);
                }
                catch (Exception ex)
                {
                    _logger.Warn("Failed to connect to pebble "+pebble.PebbleID);
                }
            }

            if (!_components.Any())
            {
                _initialized = false;
                if (pebbles.Any())
                {
                    throw new Exception("Failed to connect to any Pebbles");
                }
                else
                {
                    throw new Exception("No Pebbles found");
                }
            }
            else
            {
                _initialized = true;
            }

        }

        public bool Initialized
        {
            get { return _initialized; }
        }


        public IList<IPluginComponent> Components
        {
            get { return _components; }
        }

        public void Dispose()
        {
            if (_components != null)
            {
                foreach (var component in _components)
                {
                    component.Dispose();
                }
            }
        }
    }
}
