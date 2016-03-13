using System;
using System.Collections.Generic;

using Ninject;
using Ninject.Modules;

using MrGibbs.Configuration;
using MrGibbs.Contracts;
using MrGibbs.Contracts.Infrastructure;
using MrGibbs.Infrastructure;

namespace MrGibbs
{
    /// <summary>
    /// Bootstrap for the MrGibbs application
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
			System.Console.WriteLine("Initializing Mr. Gibbs Kernel...");

			using (var kernel = Configure ()) 
			{
				var logger = kernel.Get<ILogger> ();
				logger.Info ("Kernel Configuration Complete");

				AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
					logger.Fatal ("Unhandled Application Exception", e.ExceptionObject);
				};

				//catch ctrl-c so that we can do a proper dispose & cleanup
				Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e) {
					logger.Info ("Received Cancel Key, exiting");
					e.Cancel = true;
					kernel.Get<Supervisor> ().Exit ();
				};

				logger.Info ("Configuring Supervisor");
				bool isFirst = true;
				using (var supervisor = kernel.Get<Supervisor> ()) {
					do {
						logger.Info ("Initializing Supervisor");
						supervisor.Initialize ();
						if (isFirst) {
							var config = ConfigurationHelper.GenerateDefaultConfiguration ();
							logger.Info ("Default Configuration" + System.Environment.NewLine + config);
							isFirst = false;
						}
					}
					while (supervisor.Run ());//if run returns true then we need to restart, if false then it wants to close
				}

				logger.Info ("Shutting down");
			}
			Console.WriteLine ("Done");
        }

        /// <summary>
        /// Configures the DI container
        /// </summary>
        /// <returns>configured kernel</returns>
        static IKernel Configure()
        {
            var kernel = new StandardKernel();

			//TODO: Move this to a module
			kernel.Bind<IClock> ().To<LinuxSystemClock> ().InSingletonScope ();

            //infrastructure modules
			kernel.Load(new List<NinjectModule>()
            {
				new LoggingModule(new List<string>(){"Log.config"})
            });

			var logger = kernel.Get<ILogger> ();

			logger.Info ("Loading Plugins...");
			//plugins
			kernel.Load (new string[] { "*.Plugin.dll" });

			logger.Info ("Loading Core...");
			//core services/controllers
            kernel.Bind<IRaceController>().To<RaceController>()
                .InSingletonScope()
                .WithConstructorArgument("autoRoundMarkDistanceMeters", AppConfig.AutoRoundMarkDistanceMeters);
			kernel.Bind<Supervisor>().ToSelf()
                .InSingletonScope()
                .WithConstructorArgument("cycleTime", AppConfig.TargetCycleTime);
                
            return kernel;
        }
    }
}
