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
            _components = new List<IPluginComponent>();
        }

        public void Initialize(PluginConfiguration configuration)
        {
            _initialized = false;
            configuration.Calculators.Add(new VmgCalculator(_logger,this));
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
