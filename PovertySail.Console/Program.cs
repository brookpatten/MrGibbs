﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Ninject;
using Ninject.Modules;
using PovertySail.Configuration;
using PovertySail.Contracts;
using PovertySail.Contracts.Infrastructure;
using PovertySail.Persistence.Migrations;

namespace PovertySail.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("Starting Up...");

            IKernel kernel;
            try
            {
                kernel = Configure();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Exception during startup, "+ex.Message);
                System.Console.WriteLine(ex.StackTrace);
                return;
            }

			using (var supervisor = kernel.Get<Supervisor> ()) {
				supervisor.Initialize ();
				supervisor.Run ();
			}
        }

        static IKernel Configure()
        {
            var kernel = new StandardKernel();
            kernel.Load(new List<NinjectModule>()
            {
                new LoggingModule(new List<string>(){"Log.config"}),
                new PersistenceModule(),
                new PluginModule()
            });

            kernel.Bind<IRaceController>().To<RaceController>().InSingletonScope();

            kernel.Bind<Supervisor>().ToSelf()
                .InSingletonScope()
                .WithConstructorArgument("sleepTime", AppConfig.SleepTime);
                

            return kernel;
        }
    }
}
