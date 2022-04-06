namespace ServerApp.Database
{
    /// <summary>
    /// Interface to DBContext with inner scope.
    /// </summary>
    public interface IDatabase : IDisposable
    {
        /// <summary>
        /// Gets DB Context.
        /// </summary>
        ApplicationDbContext Context { get; }
    }
}