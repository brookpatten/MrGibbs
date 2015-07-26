using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PovertySail.Contracts;
using PovertySail.Contracts.Infrastructure;

namespace PovertySail.Calculators
{
    public class CalculatorPlugin:IPlugin
    {
        private ILogger _logger;
        private bool _initialized = false;
        private IList<IPluginComponent> _components; 

        public CalculatorPlugin(ILogger logger)
        {
            _logger = logger;
        }

        public void Initialize(PluginConfiguration configuration, ISystemController systemController, IRaceController raceController)
        {
            _components = new List<IPluginComponent>();
        
            _initialized = false;
            
            //do init here

            _initialized = true;
        }

        public bool Initialized
        {
            get { return _initialized; }
        }


        public IList<IPluginComponent> Components
        {
            get { return _components; }
        }

        public void Dispose()
        {
            if (_components != null)
            {
                foreach (var component in _components)
                {
                    component.Dispose();
                }
            }
        }
    }
}
