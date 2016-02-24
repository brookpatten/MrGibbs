using MrGibbs.Models;

namespace MrGibbs.Contracts
{
    /// <summary>
    /// a component of a plugin which calculates some value based on other values
    /// calculators should not perform any IO with hardware, disk, or network
    /// </summary>
    public interface ICalculator : IPluginComponent
    {
        void Calculate(State state);
    }
}
