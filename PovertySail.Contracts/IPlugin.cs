using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PovertySail.Contracts
{
    public interface IPlugin
    {
        void Initialize(PluginConfiguration configuration);
        bool Initialized { get; }
    }
}
