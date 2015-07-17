using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PovertySail.Contracts
{
    public class PluginConfiguration
    {
        public PluginConfiguration()
        {
            Plugins = new List<IPlugin>();
            DashboardViewers = new List<IViewer>();
            Recorders = new List<IRecorder>();
            Sensors = new List<ISensor>();
            Calculators = new List<ICalculator>();
        }

        public IList<IPlugin> Plugins { get; set; } 
        public IList<IViewer> DashboardViewers { get; set; }
        public IList<IRecorder> Recorders { get; set; }
        public IList<ISensor> Sensors { get; set; } 
        public IList<ICalculator> Calculators { get; set; } 
    }
}
