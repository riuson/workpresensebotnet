using Telegram.Bot;
using Telegram.Bot.Types;

namespace ServerApp.ChatBot
{
    /// <summary>
    /// Interface for message handlers.
    /// </summary>
    public interface IMessageHandler
    {
        /// <summary>
        /// Process message from bot client.
        /// </summary>
        /// <param name="botClient">A bot api client interface.</param>
        /// <param name="receivedMessage">Received message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        Task ProcessTextMessage(
            ITelegramBotClient botClient,
            Message receivedMessage,
            CancellationToken cancellationToken);
    }
}