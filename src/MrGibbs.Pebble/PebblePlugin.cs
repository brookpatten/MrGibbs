using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using PebbleSharp.Mono.BlueZ5;
using PebbleSharp.Core.NonPortable;
using PebbleSharp.Core.Bundles;
using MrGibbs.Contracts;
using MrGibbs.Contracts.Infrastructure;
using Mono.BlueZ.DBus;

namespace MrGibbs.Pebble
{
    public class PebblePlugin:IPlugin
    {
        private ILogger _logger;
        private bool _initialized = false;
        private IList<IPluginComponent> _components;
		private PebbleManager _manager;

        private string _pbwPath /*= "Mr._Gibbs.pbw"*/;
		private string _btAdapterName;

		public PebblePlugin(string pbwPath,string btAdapterName,ILogger logger,DBusConnection connection)
        {
			_pbwPath = pbwPath;
			_btAdapterName = btAdapterName;
            _logger = logger;
			_manager = new PebbleManager (connection);
        }

        public void Initialize(PluginConfiguration configuration, Action<Action<ISystemController, IRaceController>> queueCommand)
        {
            _components = new List<IPluginComponent>();
        
            //scan for pebbles
			var pebbles = _manager.Detect (null, true);

			_logger.Info ("Found " + pebbles.Count + " Pebbles");

            if(pebbles.Any())
            {
                if (!string.IsNullOrEmpty(_pbwPath) && File.Exists(_pbwPath))
                {
                    using (var stream = new FileStream(_pbwPath, FileMode.Open))
                    {
                        using (var zip = new Zip())
                        {
                            zip.Open(stream);
							foreach (var pebble in pebbles) 
							{
								stream.Position = 0;
								InitializeViewer(pebble, zip, queueCommand, configuration);
							}
                        }
                    }
                }
                else
                {
                    throw new FileNotFoundException("Could not find " + _pbwPath);
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

		private void InitializeViewer(PebbleSharp.Core.Pebble pebble,PebbleSharp.Core.IZip zip, Action<Action<ISystemController, IRaceController>> queueCommand,PluginConfiguration configuration)
		{
			try
			{
				var viewer = new PebbleViewer(_logger, this, pebble,zip,queueCommand);

				_components.Add(viewer);
				configuration.DashboardViewers.Add(viewer);
			}
			catch (Exception ex)
			{
				_logger.Error("Failed to connect to pebble "+pebble.PebbleID,ex);
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
			_manager.Dispose ();
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
