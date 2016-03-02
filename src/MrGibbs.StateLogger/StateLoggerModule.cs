using System;

using MrGibbs.Contracts;
using MrGibbs.Configuration;

using Ninject.Modules;

namespace MrGibbs.StateLogger
{
	public class StateLoggerModule : NinjectModule
	{
		public override void Load ()
		{
			Kernel.Bind<IPlugin> ()
				  .To<StateLoggerPlugin> ()
				  .InSingletonScope ()
			      .WithConstructorArgument ("dataPath", ConfigurationHelper.ReadStringAppSetting("DataPath",ConfigurationHelper.GetExecutingAssemblyFolder()));
		}
	}
}

