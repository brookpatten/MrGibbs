using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MrGibbs.Contracts;
using MrGibbs.Contracts.Infrastructure;

namespace MrGibbs.Calculators
{
    /// <summary>
    /// general collection of calculation plugins, possibly consider breaking apart into seperate assemblies
    /// if the need arises to only load some of these
    /// </summary>
    public class CalculatorPlugin:IPlugin
    {
        private ILogger _logger;
        private bool _initialized = false;
        private IList<IPluginComponent> _components; 

        public CalculatorPlugin(ILogger logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public void Initialize(PluginConfiguration configuration, Action<Action<ISystemController, IRaceController>> queueCommand)
        {
            _components = new List<IPluginComponent>();
        
            _initialized = false;

            var distanceCalc = new MarkCalculator(_logger, this);
            _components.Add(distanceCalc);
            configuration.Calculators.Add(distanceCalc);

            var tackCalc = new TackCalculator(_logger, this);
            _components.Add(tackCalc);
            configuration.Calculators.Add(tackCalc);



            _initialized = true;
        }

        /// <inheritdoc />
        public bool Initialized
        {
            get { return _initialized; }
        }

        /// <inheritdoc />
        public IList<IPluginComponent> Components
        {
            get { return _components; }
        }

        /// <inheritdoc />
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
