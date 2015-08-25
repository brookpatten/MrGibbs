using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MrGibbs.Contracts;
using MrGibbs.Contracts.Infrastructure;

namespace MrGibbs.MagneticVariation
{
    public class MagneticVariationPlugin:IPlugin
    {
        private ILogger _logger;
        private bool _initialized = false;
        private IList<IPluginComponent> _components;
        private TSAGeoMag _tsaGeoMag;

        public MagneticVariationPlugin(ILogger logger)
        {
            _logger = logger;
        }

        public void Initialize(PluginConfiguration configuration, Action<Action<ISystemController, IRaceController>> queueCommand)
        {
            _components = new List<IPluginComponent>();
        
            _initialized = false;

            _tsaGeoMag = new TSAGeoMag(_logger);

            var magvarCalc = new MagneticVariationCalculator(_logger, this, _tsaGeoMag);
            _components.Add(magvarCalc);
            configuration.Calculators.Add(magvarCalc);

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
