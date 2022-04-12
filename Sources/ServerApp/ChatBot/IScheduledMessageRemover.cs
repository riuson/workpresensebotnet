namespace ServerApp.ChatBot;

/// <summary>
/// Interface of service for delayed removing of messages.
/// </summary>
public interface IScheduledMessageRemover
{
    /// <summary>
    /// Prepare message <paramref name="messageId"></paramref> to remove after <paramref name="period"></paramref> elapsed.
    /// </summary>
    /// <param name="chatId">Id of chat.</param>
    /// <param name="messageId">Id of message to remove.</param>
    /// <param name="period">Period after that message should be removed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RemoveAfterAsync(
        long chatId,
        long messageId,
        TimeSpan period,
        CancellationToken cancellationToken);

    /// <summary>
    /// Prepare message <paramref name="messageId"></paramref> to remove after <paramref name="time"></paramref>.
    /// </summary>
    /// <param name="chatId">Id of chat.</param>
    /// <param name="messageId">Id of message to remove.</param>
    /// <param name="time">Time after that message should be removed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RemoveAfterAsync(
        long chatId,
        long messageId,
        DateTime time,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets collection of messages what can be removed now.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation.
    ///     The task result contains collection of messages.
    /// </returns>
    Task<IEnumerable<(long chatId, long messageId)>> GetMessagesForRemovingAsync(
        CancellationToken cancellationToken);
}