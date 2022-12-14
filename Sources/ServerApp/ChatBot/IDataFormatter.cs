using ServerApp.Entities;
using Telegram.Bot.Types.ReplyMarkups;

namespace ServerApp.ChatBot
{
    /// <summary>
    /// Interface for formatting data before sending to users.
    /// </summary>
    public interface IDataFormatter
    {
        /// <summary>
        /// Format user's location statistics data.
        /// </summary>
        /// <param name="chats">Source chats.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Formatted data.</returns>
        Task<string> FormatStats(
            IEnumerable<Chat> chats,
            CancellationToken cancellationToken);

        /// <summary>
        /// Format user's web hooks.
        /// </summary>
        /// <param name="hooks">Source web hooks data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Formatted data.</returns>
        Task<InlineKeyboardMarkup> FormatHooksKeyboardMarkup(
            Dictionary<long, Guid> hooks,
            CancellationToken cancellationToken);
    }
}