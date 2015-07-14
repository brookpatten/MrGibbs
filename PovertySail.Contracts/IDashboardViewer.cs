using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PovertySail.Models;

namespace PovertySail.Contracts
{
    public interface IDashboardViewer : IPluginComponent
    {
        void Update(Dashboard dashboard);
        event EventHandler OnStartCountdown;
        event EventHandler OnSyncCountdown;
        event EventHandler OnStopCountdown;
        event EventHandler OnSetMark;
    }
}
