using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PovertySail.Models;

namespace PovertySail.Contracts
{
    public interface IViewer : IPluginComponent
    {
        void Update(State state);
        event EventHandler OnStartCountdown;
        event EventHandler OnSyncCountdown;
        event EventHandler OnStopCountdown;
        event EventHandler OnSetMark;
    }
}
