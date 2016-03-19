using Mono.Linux.I2C;
using Ninject.Modules;

namespace MrGibbs.Configuration
{
	public class I2CModule:NinjectModule
	{
        public override void Load()
		{
			Kernel.Bind<I2CBus> ()
				.ToSelf()
				.InSingletonScope()
				.WithConstructorArgument("index", AppConfig.I2CAddress);
		}
	}
}