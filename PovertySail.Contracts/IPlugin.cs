using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PovertySail.Contracts
{
    public interface IPlugin:IDisposable
    {
        void Initialize(PluginConfiguration configuration, ISystemController systemController, IRaceController raceController);
        bool Initialized { get; }
        IList<IPluginComponent> Components { get; } 
    }
}
