using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mono.Data.Sqlite;
using Ninject.Modules;
using MrGibbs.Persistence.Migrations;

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
