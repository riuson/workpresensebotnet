using ServerApp.Entities;

namespace ServerApp.ChatBot
{
    /// <summary>
    /// Interface for managing by pinned messages.
    /// </summary>
    public interface IPinnedMessagesManager
    {
        /// <summary>
        /// Pin new message in the chat.
        /// </summary>
        /// <param name="chatId">Id of chat.</param>
        /// <param name="messageId">Id of new message.</param>
        /// <param name="messageType">Type of message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task NewAsync(
            long chatId,
            long messageId,
            MessageType messageType,
            CancellationToken cancellationToken);

        /// <summary>
        /// Mark chat to update pinned messages. Update processed in separate task.
        /// </summary>
        /// <param name="chatId">Id of chat.</param>
        void MarkChat(long chatId);

        /// <summary>
        /// Gets <see cref="ManualResetEventSlim"/> for chat.
        /// </summary>
        /// <param name="chatId">Id of chat.</param>
        /// <returns>Event for specified chat.</returns>
        ManualResetEventSlim GetChatEvent(long chatId);

        /// <summary>
        /// Gets collection of chatIds and corresponding <see cref="ManualResetEventSlim"/>.
        /// </summary>
        /// <returns>collection of chatIds and corresponding <see cref="ManualResetEventSlim"/>.</returns>
        IEnumerable<(long chatId, ManualResetEventSlim mre)> GetChatEvents();
    }
}