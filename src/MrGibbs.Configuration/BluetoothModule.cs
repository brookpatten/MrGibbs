using Mono.BlueZ.DBus;
using Ninject.Modules;
using Ninject;

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

		public override void Unload ()
		{
			var connection = Kernel.Get<DBusConnection> ();
			connection.Dispose ();
			base.Unload ();
		}
	}
}

