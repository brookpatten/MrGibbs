using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using PebbleSharp.Net45;
using PebbleSharp.Core.NonPortable;
using PebbleSharp.Core.Bundles;
using MrGibbs.Contracts;
using MrGibbs.Contracts.Infrastructure;

namespace MrGibbs.Pebble
{
    public class PebblePlugin:IPlugin
    {
        private ILogger _logger;
        private bool _initialized = false;
        private IList<IPluginComponent> _components;

#if !WINDOWS
        private const string _pbwPath = "/home/pi/dev/mrgibbs/MrGibbs.Pebble/Mr._Gibbs.pbw";
#endif

#if WINDOWS
        private const string _pbwPath = "Mr._Gibbs.pbw";
#endif

        public PebblePlugin(ILogger logger)
        {
            _logger = logger;
        }

        public void Initialize(PluginConfiguration configuration, Action<Action<ISystemController, IRaceController>> queueCommand)
        {
            _components = new List<IPluginComponent>();
        
            //scan for pebbles
			var pebbles = PebbleNet45.Detect (null, false);

            AppBundle bundle=null;

            if(pebbles.Any())
            {
                if (!string.IsNullOrEmpty(_pbwPath) && File.Exists(_pbwPath))
                {
                    using (var stream = new FileStream(_pbwPath, FileMode.Open))
                    {
                        using (var zip = new Zip())
                        {
                            zip.Open(stream);
                            bundle = new AppBundle();
                            stream.Position = 0;
                            bundle.Load(stream, zip);
                            _logger.Info("Loaded Pebble Application " + bundle.AppInfo.UUID.ToString());
                        }
                    }
                }
                else
                {
                    throw new FileNotFoundException("Could not find " + _pbwPath);
                }
            }
            
            //add a viewer for each pebble
            foreach (var pebble in pebbles)
            {
                try
                {
                    var viewer = new PebbleViewer(_logger, this, pebble,bundle,queueCommand);
                    
                    _components.Add(viewer);
                    configuration.DashboardViewers.Add(viewer);
                }
                catch (Exception ex)
                {
					_logger.Error("Failed to connect to pebble "+pebble.PebbleID,ex);
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
