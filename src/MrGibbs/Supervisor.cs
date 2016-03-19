using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using MrGibbs.Contracts;
using MrGibbs.Contracts.Infrastructure;
using MrGibbs.Models;

namespace MrGibbs
{
    /// <summary>
    /// Handles threading and plugin maintenance throughout the execution of the application.
    /// In the event that a plugin throws an exception (due to hardware, software, whatever) it will attempt to recover that plugin
    /// and hopefully leave the existing plugins still functional
    /// </summary>
	public class Supervisor:IDisposable,ISystemController,ICommandable
    {
        private ILogger _logger;
        private PluginConfiguration _configuration;
		private int _cycleTime;
        private IRaceController _raceController;
        private State _state;
        private IList<IPlugin> _allPlugins;
        private bool _run;
        private bool _restart;
        private Queue<Action<ISystemController, IRaceController>> _commands;

		public Supervisor(ILogger logger,IList<IPlugin> plugins, int cycleTime, IRaceController raceController)
        {
            _raceController = raceController;
            _cycleTime = cycleTime;
            _logger = logger;
            _allPlugins = plugins;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.Fatal("AppDomain Unhandled Exception", e.ExceptionObject);
        }

        /// <summary>
        /// intiialize the plugins
        /// </summary>
        public void Initialize()
        {
            _commands = new Queue<Action<ISystemController, IRaceController>>();
            _configuration = new PluginConfiguration();
            _configuration.Plugins = _allPlugins.Select(x=>x).ToList();

            _state = _raceController.State;

            var failed = new List<IPlugin>();
            foreach (var plugin in _configuration.Plugins)
            {
                try
                {
                    _logger.Info("Initializing Plugin " + plugin.GetType().Name);
                    InitializePlugin(plugin);
                }
                catch (Exception ex)
                {
                    _logger.Fatal("Exception initializing plugin "+plugin.GetType().Name, ex);
                    failed.Add(plugin);
                }
            }
            foreach (var plugin in failed)
            {
                EvictPlugin(_configuration,plugin,false);
            }

            //remove any plugins that failed to initialize
            //_configuration.Plugins = _configuration.Plugins.Where(x => x.Initialized).ToList();
            foreach (var plugin in _configuration.Plugins)
            {
                _logger.Info(plugin.GetType().Name + " Initialized OK");
            }
            
        }

        /// <summary>
        /// initialize a single plugin
        /// </summary>
        /// <param name="plugin"></param>
        private void InitializePlugin(IPlugin plugin)
        {
            plugin.Initialize(_configuration, QueueCommand);
        }

        /// <inheritdoc />
        public void QueueCommand(Action<ISystemController,IRaceController> command)
        {
            lock(_commands)
            {
                _commands.Enqueue(command);
            }
        }

        /// <summary>
        /// the main thread loop of the application
        /// </summary>
        /// <returns>returns a boolean indicating whether the thread should be restarted</returns>
        public bool Run()
        {
            _logger.Info("Supervisor is running");
            _run = true;
            _restart = false;
            int operationCount = 1;
            while (_run && operationCount>0)
            {
				_state.Clear();
                _state.SystemTime = DateTime.UtcNow;
                
                operationCount = 0;
                lock (_state)
                {
                    _state.CycleMessages();
                }

                IList<IPlugin> erroredPlugins = new List<IPlugin>();

                foreach (var sensor in _configuration.Sensors)
                {
                    try
                    {
                        lock (_state)
                        {
                            sensor.Update(_state);
                        }
                        operationCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Exception updating sensor "+sensor.GetType().Name,ex);
                        if (!erroredPlugins.Contains(sensor.Plugin))
                        {
                            erroredPlugins.Add(sensor.Plugin);
                        }
                    }
                }

                foreach (var calculator in _configuration.Calculators)
                {
                    try
                    {
                        lock (_state)
                        {
                            calculator.Calculate(_state);
                        }
                        operationCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Exception updating calculator " + calculator.GetType().Name,ex);
                        if (!erroredPlugins.Contains(calculator.Plugin))
                        {
                            erroredPlugins.Add(calculator.Plugin);
                        }
                    }
                }

                foreach (var recorder in _configuration.Recorders)
                {
                    try
                    {
						lock(_state) 
						{
							recorder.Record (_state);
						}
                        operationCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Exception updating recorder " + recorder.GetType().Name,ex);
                        if (!erroredPlugins.Contains(recorder.Plugin))
                        {
                            erroredPlugins.Add(recorder.Plugin);
                        }
                    }
                }

                foreach (var viewer in _configuration.DashboardViewers)
                {
                    try
                    {
                        lock (_state)
                        {
                            viewer.Update(_state);
                        }
                        operationCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Exception updating viewer " + viewer.GetType().Name,ex);
                        if (!erroredPlugins.Contains(viewer.Plugin))
                        {
                            erroredPlugins.Add(viewer.Plugin);
                        }
                    }
                }

                //execute any pending commands from the plugins
                lock (_commands)
                {
                    lock (_state)
                    {
                        while (_commands.Any())
                        {
                            var command = _commands.Dequeue();
                            try
                            {
                                command(this, _raceController);
                            }
                            catch(Exception ex)
                            {
                                _state.AddMessage(MessageCategory.System, MessagePriority.High, 5, "Command Failed");
                                _logger.Error("Failed to execute command " + command.ToString(),ex);
                            }
                        }

                        _raceController.ProcessMarkRoundings();

						if(_state.Message!=null)
                        {
                            _logger.Info(_state.Message.Text);
                        }
                    }
                }

                //attempt to reinitialize any plugins that encountered errors
                foreach (var plugin in erroredPlugins)
                {
                    EvictPlugin(_configuration,plugin,true);
                }

				DateTime finishedAt = DateTime.UtcNow;
				var elapsed =  finishedAt - _state.SystemTime;
				if (elapsed.TotalMilliseconds > _cycleTime) 
				{
					_logger.Warn ("Cycle exceeded target cycle time: " + elapsed.TotalMilliseconds);
				} 
				else 
				{
					var sleepTime = _cycleTime - (int)elapsed.TotalMilliseconds;
					_logger.Debug("Sleeping for "+sleepTime);
					Thread.Sleep(sleepTime);
				}
			}

            return _restart;
        }

        /// <summary>
        /// evicts a plugin and optionally reloads it
        /// </summary>
        /// <param name="configuration">the configuration object</param>
        /// <param name="plugin">the plugin to evict</param>
        /// <param name="reinitialize">whether to re-initialize the plugin or leave it out</param>
        private void EvictPlugin(PluginConfiguration configuration,IPlugin plugin,bool reinitialize)
        {
            _logger.Warn("Evicting "+plugin.GetType().Name+" "+(reinitialize? "with" : "without")+" reinitialize");

            configuration.Sensors = configuration.Sensors.Where(x => x.Plugin != plugin).ToList();
            configuration.Calculators = configuration.Calculators.Where(x => x.Plugin != plugin).ToList();
            configuration.Recorders = configuration.Recorders.Where(x => x.Plugin != plugin).ToList();
            configuration.DashboardViewers = configuration.DashboardViewers.Where(x => x.Plugin != plugin).ToList();
            
            if (reinitialize)
            {
                //allow it to re-add the components and attempt to restart
                InitializePlugin(plugin);

                if (!plugin.Initialized)
                {
                    EvictPlugin(configuration,plugin,false);
                }
            }
            else
            {
                //remove it entirely
                configuration.Plugins = configuration.Plugins.Where(x => x != plugin).ToList();
            }
        }

		public void Dispose()
		{
			if (_configuration != null) {
				_configuration.Dispose ();
			}
		}

        /// <inheritdoc />
        public void Calibrate()
        {
            _logger.Info("Calibrating...");
            lock(_state)
            {
                foreach(var sensor in _configuration.Sensors)
                {
                    _logger.Info("Calibrating " + sensor.Plugin.GetType().Name);
                    sensor.Calibrate();
                }
            }
            _logger.Info("Calibration Complete");
        }

        /// <inheritdoc />
        public void Restart()
        {
            _logger.Info("Restart");
            _restart = true;
            _run = false;
        }

        /// <inheritdoc />
        public void Reboot()
        {
            _logger.Info("Reboot");

            _restart = false;
            
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.EnableRaisingEvents = false;
            proc.StartInfo.FileName = "sudo";
            proc.StartInfo.Arguments = "shutdown -r now";
            proc.Start();
            
            _run = false;
        }

		/// <inheritdoc />
		public void Exit()
		{
			_logger.Info("Exit");

			_restart = false;
			_run = false;
		}

        /// <inheritdoc />
        public void Shutdown()
        {
            _logger.Info("Shutdown");

            _restart = false;

            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.EnableRaisingEvents = false;
            proc.StartInfo.FileName = "sudo";
            proc.StartInfo.Arguments = "shutdown -h now";
            proc.Start();

            _run = false;
        }
    }
}
