using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject.Modules;
using PovertySail.Contracts.Infrastructure;
using PovertySail.Infrastructure;

using log4net;
using Ninject.Activation;

namespace PovertySail.Configuration
{
    public class LoggingModule:NinjectModule
    {
        private IList<string> _xmlConfigFiles;

        public LoggingModule(IList<string> xmlConfigFiles)
        {
            _xmlConfigFiles = xmlConfigFiles;
        }

        public override void Load()
        {
            ConfigureGlobalLog4Net();

            Bind<ILog>().ToMethod(x => LogManager.GetLogger(GetParentTypeName(x)))
                .InTransientScope();

            Bind<ILogger>().To<Log4NetLogger>()
                .InTransientScope();
        }

        private void ConfigureGlobalLog4Net()
        {
            foreach (var file in _xmlConfigFiles)
            {
                log4net.Config.XmlConfigurator.Configure(new FileInfo(file));
            }
        }

        private string GetParentTypeName(IContext context)
        {
            string interfaceShortName;
            string interfaceFullName;
            string concreteFullName;

            if (context.Request.ParentContext.Request.ParentContext != null)
            {
                interfaceFullName = context.Request.ParentContext.Request.ParentContext.Request.Service.FullName;
                interfaceShortName = context.Request.ParentContext.Request.ParentContext.Request.Service.Name;
                concreteFullName = context.Request.ParentRequest.Target.Member.DeclaringType.FullName;
            }
            else
            {
                interfaceFullName = context.Request.ParentContext.Request.Service.FullName;
                interfaceShortName = context.Request.ParentContext.Request.Service.Name;
                concreteFullName = context.Request.Target.Member.DeclaringType.FullName;
            }

            if (interfaceFullName != concreteFullName)
            {
                return string.Format("{0}:{1}", concreteFullName, interfaceShortName);
            }
            else
            {
                return concreteFullName;
            }
        }
    }
}
