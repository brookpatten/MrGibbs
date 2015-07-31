using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PovertySail.Contracts;
using PovertySail.Contracts.Infrastructure;
using PovertySail.Models;

namespace PovertySail.Console
{
	public class Supervisor:IDisposable,ISystemController
    {
        private ILogger _logger;
        private PluginConfiguration _configuration;
        private int _sleepTime;
        private IRaceController _raceController;
        private State _state;
        private IList<IPlugin> _allPlugins;
        private bool _run;
        private bool _restart;
        private Queue<Action<ISystemController, IRaceController>> _commands;

        public Supervisor(ILogger logger,IList<IPlugin> plugins, int sleepTime, IRaceController raceController)
        {
            _raceController = raceController;
            _sleepTime = sleepTime;
            _logger = logger;
            _allPlugins = plugins;
        }

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

        private void InitializePlugin(IPlugin plugin)
        {
            plugin.Initialize(_configuration, QueueCommand);
        }

        private void QueueCommand(Action<ISystemController,IRaceController> command)
        {
            lock(_commands)
            {
                _commands.Enqueue(command);
            }
        }

        public bool Run()
        {
            _logger.Info("Supervisor is running");
            _run = true;
            _restart = false;
            int operationCount = 1;
            _state.AddMessage(MessageCategory.System, MessagePriority.Normal, 5, "Startup Complete");
            while (_run && operationCount>0)
            {
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
                        _logger.Error("Exception updating sensor "+sensor.GetType().Name);
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
                        _logger.Error("Exception updating calculator " + calculator.GetType().Name);
                        if (!erroredPlugins.Contains(calculator.Plugin))
                        {
                            erroredPlugins.Add(calculator.Plugin);
                        }
                    }
                }

                //foreach (var recorder in _configuration.Recorders)
                //{
                //    try
                //    {
                //        operationCount++;
                //    }
                //    catch (Exception ex)
                //    {
                //        _logger.Error("Exception updating recorder " + recorder.GetType().Name);
                //        if (!erroredPlugins.Contains(recorder.Plugin))
                //        {
                //            erroredPlugins.Add(recorder.Plugin);
                //        }
                //    }
                //}

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
                        _logger.Error("Exception updating viewer " + viewer.GetType().Name);
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
                    }
                }

                //attempt to reinitialize any plugins that encountered errors
                foreach (var plugin in erroredPlugins)
                {
                    EvictPlugin(_configuration,plugin,true);
                }

                _logger.Debug("Sleeping");
                Thread.Sleep(_sleepTime);
            }

            return _restart;
        }

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

        public void Restart()
        {
            _logger.Info("Restart");
            _restart = true;
            _run = false;
        }

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
