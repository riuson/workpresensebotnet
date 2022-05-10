using ServerApp.Entities;

namespace ServerApp.Database
{
    /// <summary>
    /// Interface to DBContext with inner scope.
    /// </summary>
    public interface IDatabase
    {
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
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains Ids of affected chats.
        /// </returns>
        Task<IEnumerable<long>> UpdateUserStatusAsync(
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
        Task<(bool isSuccessfull, long chatId, Status previousStatus, DateTime time)> UpdateUserStatusAsync(
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
        /// <returns>
        ///     A task that represents the asynchronous operation.
        ///     The task result contains collection of chat with references to chat statuses and users.
        /// </returns>
        Task<IEnumerable<Chat>> GetStatsAsync(
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

        /// <summary>
        /// Gets last pinned message info for chat.
        /// </summary>
        /// <param name="chatId">Chat Id.</param>
        /// <param name="messageType">Type of pinned message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Pinned message info.</returns>
        Task<(bool isSuccessfull, long messageId, DateTime time)> GetPinnedMessageAsync(
            long chatId,
            MessageType messageType,
            CancellationToken cancellationToken);

        /// <summary>
        /// Remember pinned message data.
        /// </summary>
        /// <param name="chatId">Chat Id.</param>
        /// <param name="messageId">Pinned message Id.</param>
        /// <param name="messageType">Type of pinned message.</param>
        /// <param name="time">Time of last updating.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task UpdatePinnedMessageAsync(
            long chatId,
            int messageId,
            MessageType messageType,
            DateTime time,
            CancellationToken cancellationToken);

        /// <summary>
        /// Updates user name (first, last or nickname).
        /// </summary>
        /// <param name="userId">User Id (Telegram).</param>
        /// <param name="callback">Callback that received current values and returns new values.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task UpdateUserInfoAsync(
            long userId,
            Func<(string firstName, string lastName, string nickName), (string firstName, string lastName, string
                nickName)> callback,
            CancellationToken cancellationToken);
    }
}