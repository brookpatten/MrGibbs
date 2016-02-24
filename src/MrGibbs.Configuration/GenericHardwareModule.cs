using QuadroschrauberSharp.Hardware;
using Mono.BlueZ.DBus;
using Ninject.Modules;

namespace MrGibbs.Configuration
{
	public class GenericHardwareModule:NinjectModule
	{
        /// <summary>
        /// configure common hardware resources that are used by multple plugins
        /// TODO: move this to the plugins that requre them and have it check for an existing binding before creting new ones
        /// </summary>
		public override void Load()
		{
			//TODO: find a better home for these
			//I2c is used by both the compass and accel, also likely any additional future dev boards
			Kernel.Bind<I2C> ()
				.ToSelf()
				.InSingletonScope()
				.WithConstructorArgument("index", AppConfig.I2CAddress);
			//dbus is used by both pebble and blend micro, possibly also anything else on dbus
			Kernel.Bind<DBusConnection> ()
				.ToSelf ()
				.InSingletonScope ();
		}
	}
}

