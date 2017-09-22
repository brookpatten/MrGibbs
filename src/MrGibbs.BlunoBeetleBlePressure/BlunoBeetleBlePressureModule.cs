using System;

using Ninject;
using Ninject.Modules;

using MrGibbs.Contracts;
using MrGibbs.Configuration;

namespace MrGibbs.BlunoBeetleBlePressure
{
	
	/// <summary>
	/// configures types releavant to this plugin
	/// </summary>
	public class BlunoBeetleBlePressureModule:NinjectModule
	{
		public override void Load()
		{
			string address = ConfigurationHelper.ReadStringAppSetting ("BlunoBeetleBlePressureAddress", "");

			if (!string.IsNullOrEmpty (address)) 
			{
				Kernel.LoadIfNotLoaded<BluetoothModule> ();
			}

			Kernel.Bind<IPlugin> ()
				.To<BlunoBeetleBlePressurePlugin> ()
				.InSingletonScope ()
				.WithConstructorArgument ("simulated", false/*AppConfig.SimulateSensorData*/)
				.WithConstructorArgument ("maximumDataAge", new TimeSpan (0, 0, 3))
				.WithConstructorArgument ("btAdapterName", ConfigurationHelper.ReadStringAppSetting ("BtAdapterName", "hci0"))
				.WithConstructorArgument ("deviceAddress", address);

		}

		public override void Unload ()
		{
			var connection = Kernel.Get<BlunoBeetleBlePressurePlugin> ();
			connection.Dispose ();
			base.Unload ();
		}
	}
}

