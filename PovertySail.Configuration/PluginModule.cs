using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Ninject.Modules;
using Ninject.Extensions.Conventions;

using PovertySail.Contracts;

namespace PovertySail.Configuration
{
    public class PluginModule:NinjectModule
    {
        public override void Load()
        {
            string path = Assembly.GetExecutingAssembly().Location;
            FileInfo file = new FileInfo(path);
            DirectoryInfo directory = file.Directory;

            Kernel.Bind(x =>
            {
                x.FromAssembliesMatching(new List<string>(){"PovertySail.*"})
                    .Select(y => 
                        y.GetInterfaces().Contains(typeof(IPlugin)) && y.IsClass && !y.IsInterface)
                    .BindSingleInterface()
                    .Configure(b => b.InSingletonScope());
            });
        }
    }
}
