using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PovertySail.Contracts
{
    public interface IPlugin:IDisposable
    {
        void Initialize(PluginConfiguration configuration, EventHandler onWatchButton, EventHandler onHeadingButton, EventHandler onSpeedButton);
        bool Initialized { get; }
        IList<IPluginComponent> Components { get; } 
    }
}
