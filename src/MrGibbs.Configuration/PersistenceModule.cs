using Ninject.Modules;
using Mono.Data.Sqlite;

namespace MrGibbs.Configuration
{
    public class PersistenceModule:NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind<SqliteConnection>()
                .ToConstructor(art => new SqliteConnection(AppConfig.DatabaseConnectionString))
                .InSingletonScope();

            //Kernel.Bind<IMakeRepository>().To<MakeRepository>().InRequestScope();
        }
    }
}
