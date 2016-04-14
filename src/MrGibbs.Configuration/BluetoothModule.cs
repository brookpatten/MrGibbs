using System;
using System.Threading;

using Mono.BlueZ.DBus;
using Ninject.Modules;
using Ninject;

using MrGibbs.Contracts.Infrastructure;

namespace MrGibbs.Configuration
{
	public class BluetoothModule:NinjectModule
	{
		public override void Load()
		{
			Kernel.Bind<DBusConnection> ()
			      .ToSelf ()
			      .InSingletonScope ();

			Kernel.Bind<Adapter1> ()
				  .ToMethod (x => {
					var adapterName = ConfigurationHelper.ReadStringAppSetting ("BtAdapterName", "hci0");
					var connection = x.Kernel.Get<DBusConnection> ();
					var adapter = connection.System.GetObject<Adapter1>(BlueZPath.Service, BlueZPath.Adapter (adapterName));
					return adapter;
				  }).InSingletonScope();

			//we default to true so that it works "out of the box", but once your devices are paired you
			//could turn discovery off to speed up startup time
			var doDiscovery = ConfigurationHelper.ReadBoolAppSetting ("BtEnableDiscovery", true);
			var discoveryWait = ConfigurationHelper.ReadIntAppSetting ("BtDiscoveryWait", 5);
			//HACK: very odd to be doing this sort of work in a DI module, but I don't have a better place right now
			if (doDiscovery) 
			{
				StartDiscovery (discoveryWait);
			}
		}

		public override void Unload ()
		{
			var connection = Kernel.Get<DBusConnection> ();
			connection.Dispose ();
			base.Unload ();
		}

		public void StartDiscovery (int discoveryWait)
		{
			var logger = Kernel.Get<ILogger> ();
			var adapter = Kernel.Get<Adapter1> ();
			logger.Info ("Starting Discovery");
			try 
			{
				if (!adapter.Discovering) 
				{
					adapter.StartDiscovery ();
				}
				if (discoveryWait >= 0) 
				{
					Thread.Sleep (discoveryWait * 1000);
				} 
				else 
				{
					throw new ArgumentException ("Timeout value must be >=0", "BtDiscoveryWait");
				}
				logger.Debug ("Discovery Complete");
			} 
			catch (Exception ex) 
			{
				logger.Fatal ("Exception during bluetooth discovery", ex);
			}
		}
	}
}

