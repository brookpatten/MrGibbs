using System;

using MrGibbs.Contracts;
using MrGibbs.Configuration;

using Ninject.Modules;

namespace MrGibbs.StateLogger
{
	/// <summary>
	/// A simple plugin to write various state fields to a table in a sqlite db
	/// </summary>
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

