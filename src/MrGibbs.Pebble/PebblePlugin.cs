using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using PebbleSharp.Mono.BlueZ5;
using Mono.BlueZ.DBus;

using MrGibbs.Contracts;
using MrGibbs.Contracts.Infrastructure;

namespace MrGibbs.Pebble
{
    /// <summary>
    /// pebble plugin which finds, pairs, and communicates with pebble watches
    /// </summary>
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

        /// <inheritdoc />
        public void Initialize(PluginConfiguration configuration, Action<Action<ISystemController, IRaceController>> queueCommand)
        {
            _components = new List<IPluginComponent>();
        
            //scan for pebbles, we rely on MrGibbs.Configuration.BluetoothModule to have done discovery
			var pebbles = _manager.Detect (_btAdapterName, false,true,true);

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

        /// <summary>
        /// initialize a given pebble and add it to the viewers if successful
        /// </summary>
        /// <param name="pebble"></param>
        /// <param name="zip"></param>
        /// <param name="queueCommand"></param>
        /// <param name="configuration"></param>
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

        /// <inheritdoc />
        public bool Initialized
        {
            get { return _initialized; }
        }

        /// <inheritdoc />
        public IList<IPluginComponent> Components
        {
            get { return _components; }
        }

        /// <inheritdoc />
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
