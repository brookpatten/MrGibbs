using MrGibbs.Models;

namespace MrGibbs.Contracts
{
    /// <summary>
    /// A sensor component of a plugin.  Performs hardware or network IO to gather data
    /// and add it to the current state
    /// </summary>
    public interface ISensor : IPluginComponent
    {
        /// <summary>
        /// Update the current state using the sensors data
        /// </summary>
        /// <param name="state">state to update</param>
        void Update(State state);
        /// <summary>
        /// calibrate the sensor (if necassary)
        /// </summary>
        void Calibrate();
    }
}
