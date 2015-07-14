using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PovertySail.Models;

namespace PovertySail.Contracts
{
    public interface ICalculator : IPluginComponent
    {
        void Calculate(Dashboard dashboard);
    }
}
