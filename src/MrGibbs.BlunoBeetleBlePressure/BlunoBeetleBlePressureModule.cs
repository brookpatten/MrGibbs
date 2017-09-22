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
			string portAddress = ConfigurationHelper.ReadStringAppSetting ("BlunoBeetleBlePressureAddressPort", "");
			string starboardAddress = ConfigurationHelper.ReadStringAppSetting ("BlunoBeetleBlePressureAddressStarboard", "");

			if (!string.IsNullOrEmpty (portAddress) || !string.IsNullOrEmpty(starboardAddress)) 
			{
				Kernel.LoadIfNotLoaded<BluetoothModule> ();
			}

			Kernel.Bind<IPlugin> ()
				.To<BlunoBeetleBlePressurePlugin> ()
				.InSingletonScope ()
				.WithConstructorArgument ("simulated", false/*AppConfig.SimulateSensorData*/)
				.WithConstructorArgument ("maximumDataAge", new TimeSpan (0, 0, 3))
				.WithConstructorArgument ("btAdapterName", ConfigurationHelper.ReadStringAppSetting ("BtAdapterName", "hci0"))
				.WithConstructorArgument ("portAddress", portAddress)
				.WithConstructorArgument ("starboardAddress", starboardAddress);

		}

		public override void Unload ()
		{
			var connection = Kernel.Get<BlunoBeetleBlePressurePlugin> ();
			connection.Dispose ();
			base.Unload ();
		}
	}
}

