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
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<IEnumerable<UserInfo>> GetUsersAsync(
        IEnumerable<long> userIds,
        CancellationToken cancellationToken);

    /// <summary>
    /// Updates User's State.
    /// </summary>
    /// <param name="userId">The Id of User.</param>
    /// <param name="state">The new State of User.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateUserStateAsync(
        long userId,
        UserState state,
        CancellationToken cancellationToken);

    /// <summary>
    /// Updates User's State.
    /// </summary>
    /// <param name="webHookId">The unique Id of the web hook.</param>
    /// <param name="state">The new State of User.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateUserStateAsync(
        Guid webHookId,
        UserState state,
        CancellationToken cancellationToken);
}