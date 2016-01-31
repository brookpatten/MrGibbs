using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
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

			string newestCofFile = FindNewestCofFile ();

			_tsaGeoMag = new TSAGeoMag(newestCofFile,_logger);

            var magvarCalc = new MagneticVariationCalculator(_logger, this, _tsaGeoMag);
            _components.Add(magvarCalc);
            configuration.Calculators.Add(magvarCalc);

            _initialized = true;
        }

		private string FindNewestCofFile()
		{
			string exePath = System.Reflection.Assembly.GetExecutingAssembly ().CodeBase;
			string exeDir = Path.GetDirectoryName (exePath);
			var dir = new DirectoryInfo (exeDir);
			var cofFiles = dir.GetFiles ("*.cof");
			var newest = cofFiles.OrderByDescending (x => x.CreationTimeUtc).FirstOrDefault();
			if(newest!=null)
			{
				return newest.FullName;
			}
			else
			{
				return null;
			}
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
