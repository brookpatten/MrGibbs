using System.Data;

namespace MrGibbs.Contracts.Persistence.Repositories
{
    /// <summary>
    /// repository for saving data to the database
    /// </summary>
    public interface IPluginRepository<T>
    {
		void Save(long ticks,T t);
    }
}
