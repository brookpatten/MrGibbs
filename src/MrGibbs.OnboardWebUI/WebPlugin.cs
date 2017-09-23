using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ninject;

using MrGibbs.Contracts;
using MrGibbs.Contracts.Infrastructure;
using MrGibbs.Models;

namespace MrGibbs.OnboardWebUI
{
    public class WebPlugin:IPlugin
    {
        public bool Initialized { get; private set; }
        private IViewer _viewer;
        private ILogger _logger;
		private IKernel _kernel;

		public WebPlugin(ILogger logger,IKernel kernel)
        {
            _logger = logger;
			_kernel = kernel;
        }

        public IList<IPluginComponent> Components
        {
            get
            {
                if (_viewer != null && Initialized)
                {
                    return new List<IPluginComponent>() { _viewer };
                }
                else
                {
                    return new List<IPluginComponent>();
                }
            }
        }

        public void Initialize(PluginConfiguration configuration, Action<Action<ISystemController, IRaceController>> queueCommand)
        {
            try
            {
				_viewer = new WebViewer(_logger,queueCommand, this,_kernel);

                configuration.DashboardViewers.Add(_viewer);
            }
            catch (Exception ex)
            {
                _viewer = null;
                _logger.Error("Failed to initialize onboard web ui", ex);
            }
        }

        public void Dispose()
        {
            if (this.Components != null)
            {
                foreach (var component in this.Components)
                {
                    component.Dispose();
                }
            }
        }
    }
}
