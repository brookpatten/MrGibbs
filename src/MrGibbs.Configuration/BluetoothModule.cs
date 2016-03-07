using Mono.BlueZ.DBus;
using Ninject.Modules;

namespace MrGibbs.Configuration
{
	public class BluetoothModule:NinjectModule
	{
		public override void Load()
		{
			Kernel.Bind<DBusConnection> ()
			      .ToSelf ()
			      .InSingletonScope ();
		}
	}
}

