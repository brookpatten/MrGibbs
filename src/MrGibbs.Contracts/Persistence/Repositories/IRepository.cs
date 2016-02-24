namespace MrGibbs.Contracts.Persistence.Repositories
{
    /// <summary>
    /// repository for saving data to the database
    /// </summary>
    public interface IRepository
    {
        void Save<T>(T t);
    }
}
