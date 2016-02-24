using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MrGibbs.Models;

namespace MrGibbs.Contracts
{
    /// <summary>
    /// Viewer components are intended to display State data to the user in some way
    /// Either via hardware IO or network IO
    /// </summary>
    public interface IViewer : IPluginComponent
    {
        /// <summary>
        /// Update the "view" with data from the state
        /// </summary>
        /// <param name="state"></param>
        void Update(State state);
    }
}
