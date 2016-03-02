using MrGibbs.Models;
namespace MrGibbs.Contracts
{
    /// <summary>
    /// persists data to the database
    /// not OSS yet, TBD
    /// </summary>
    public interface IRecorder : IPluginComponent
    {
		void Record (State state);
    }
}

