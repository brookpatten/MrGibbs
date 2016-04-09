using System;
using System.Data;

using MrGibbs.Contracts;
using MrGibbs.Configuration;

using Ninject.Modules;
using Ninject;

namespace MrGibbs.StateLogger
{
	/// <summary>
	/// A simple plugin to write various state fields to a table in a sqlite db
	/// </summary>
	public class StateLoggerModule : NinjectModule
	{
		public override void Load ()
		{
			Kernel.LoadIfNotLoaded<DatabaseModule> ();

			Kernel.Bind<StateRecorder> ()
				  .ToSelf ()
			      .InSingletonScope ();

			Kernel.Bind<IPlugin> ()
				  .To<StateLoggerPlugin> ()
				  .InSingletonScope ();
		}
	}
}

