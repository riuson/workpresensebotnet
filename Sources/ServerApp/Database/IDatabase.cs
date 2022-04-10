using ServerApp.Entities;

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

        /// <summary>
        /// Updates status of the user.
        /// </summary>
        /// <param name="userId">User Id (Telegram).</param>
        /// <param name="chatId">Chat Id (Telegram).</param>
        /// <param name="isPrivateChat">Flag indicating that message was received via private chat.</param>
        /// <param name="nickName">Nickname of the user.</param>
        /// <param name="firstName">First name of the user.</param>
        /// <param name="lastName">Last name of the user.</param>
        /// <param name="status">New status of the user.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Count of affected entities in the database.</returns>
        Task<int> UpdateUserStatusAsync(
            long userId,
            long chatId,
            bool isPrivateChat,
            string nickName,
            string firstName,
            string lastName,
            Status status,
            CancellationToken cancellationToken);

        /// <summary>
        /// Updates status of the user.
        /// </summary>
        /// <param name="hookId">Unique for user id of web hook.</param>
        /// <param name="status">New status of the user.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Status of operation.</returns>
        Task<(bool isSuccessfull, Status previousStatus, DateTime time)> UpdateUserStatusAsync(
            Guid hookId,
            Status status,
            CancellationToken cancellationToken);

        /// <summary>
        /// Gets stats for chats where user is registered.
        /// </summary>
        /// <param name="userId">User Id (Telegram).</param>
        /// <param name="chatId">Chat Id (Telegram).</param>
        /// <param name="isPrivateChat">Flag indicating that message was received via private chat.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Dictionary with chatIds and chat records/users/statuses.</returns>
        Task<Dictionary<long, IEnumerable<ChatStatus>>> GetStatsAsync(
            long userId,
            long chatId,
            bool isPrivateChat,
            CancellationToken cancellationToken);

        /// <summary>
        /// Gets collection of unique web hook Id for user.
        /// </summary>
        /// <param name="userId">Id of user.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Collection of unique web hook Id for user in form chatId : hookId.</returns>
        Task<Dictionary<long, Guid>> GetHooksAsync(
            long userId,
            CancellationToken cancellationToken);
    }
}