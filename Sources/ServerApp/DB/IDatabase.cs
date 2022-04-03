using ServerApp.Defs;

namespace ServerApp.DB;

/// <summary>
/// Interface to persistent data storage.
/// </summary>
public interface IDatabase
{
    /// <summary>
    /// Gets collection of <see cref="UserInfo"/> by User Id.
    /// </summary>
    /// <param name="userIds">Collection of User Ids.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of <see cref="UserInfo"/> from storage.</returns>
    Task<IEnumerable<UserInfo>> GetUsersAsync(
        IEnumerable<long> userIds,
        CancellationToken cancellationToken);
}