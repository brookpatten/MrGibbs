using System.Collections.Generic;

using Ninject.Activation;
using NLog;
using ILogger = MrGibbs.Contracts.Infrastructure.ILogger;
using Ninject.Modules;

using MrGibbs.Infrastructure;

namespace MrGibbs.Configuration
{
    /// <summary>
    /// load and configure the NLog logger
    /// </summary>
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
