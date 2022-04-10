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
        /// Update previously pinned message in the chat. If there is no pinned messages, create new.
        /// </summary>
        /// <param name="chatId">Id of chat.</param>
        /// <param name="text">Text of message.</param>
        /// <param name="messageType">Type of message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task UpdateAsync(
            long chatId,
            string text,
            MessageType messageType,
            CancellationToken cancellationToken);
    }
}