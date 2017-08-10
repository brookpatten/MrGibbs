using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MrGibbs.Contracts;
using MrGibbs.Contracts.Infrastructure;
using MrGibbs.Models;

using Ninject;
using Ninject.Modules;

namespace MrGibbs.OnboardWebUI
{
    public class WebModule : NinjectModule
    {
        public override void Load()
        {
            Kernel.Bind<IPlugin>().To<WebPlugin>()
                .InSingletonScope();

        }
    }
}
