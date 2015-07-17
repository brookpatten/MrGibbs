using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject.Modules;
using PovertySail.Infrastructure;

using Ninject.Activation;
using NLog;
using ILogger = PovertySail.Contracts.Infrastructure.ILogger;

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
            Bind<NLog.ILogger>().ToMethod(x => LogManager.GetLogger(GetParentTypeName(x)))
                .InTransientScope();

            Bind<ILogger>().To<NLogLogger>()
                .InTransientScope();
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
