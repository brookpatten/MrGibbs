using System;
using System.Data;

using MrGibbs.Contracts;
using MrGibbs.Configuration;

using Ninject.Modules;
using Ninject;

namespace MrGibbs.PolarCalculator
{
	/// <summary>
	/// A simple plugin to write various state fields to a table in a sqlite db
	/// </summary>
	public class PolarModule : NinjectModule
	{
		public override void Load ()
		{
			Kernel.LoadIfNotLoaded<DatabaseModule> ();

			Kernel.Bind<PolarRecorder> ()
				  .ToSelf ()
			      .InSingletonScope ()
			      .WithConstructorArgument("connection",c=>c.Kernel.Get<IDbConnection>(DatabaseModule.Databases.Persistant.ToString()));

			Kernel.Bind<IPlugin> ()
				  .To<PolarPlugin> ()
				  .InSingletonScope ();
		}
	}
}

