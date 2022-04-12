namespace ServerApp.ChatBot;

/// <summary>
/// Service for delayed removing of messages.
/// </summary>
public class ScheduledMessageRemover : IScheduledMessageRemover
{
    private readonly List<(long chatId, long messageId, DateTime time)> items = new ();
    private readonly SemaphoreSlim semaphore = new (1);

    /// <inheritdoc />
    public async Task<IEnumerable<(long chatId, long messageId)>> GetMessagesForRemovingAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            if (await this.semaphore.WaitAsync(TimeSpan.FromSeconds(1), cancellationToken))
            {
                var result = this.items
                    .Where(x => x.time < DateTime.Now)
                    .Select(x => (x.chatId, x.messageId))
                    .ToArray();

                this.items.RemoveAll(x => result.Any(y => y.messageId == x.messageId));

                this.semaphore.Release();
                return result;
            }
        }
        catch (OperationCanceledException)
        {
        }

        return Array.Empty<(long chatId, long messageId)>();
    }

    /// <inheritdoc />
    public async Task RemoveAfterAsync(
        long chatId,
        long messageId,
        TimeSpan period,
        CancellationToken cancellationToken)
    {
        try
        {
            await this.semaphore.WaitAsync(cancellationToken);
            this.items.Add((chatId, messageId, DateTime.Now + period));
            this.semaphore.Release();
        }
        catch (OperationCanceledException)
        {
        }
    }

    /// <inheritdoc />
    public async Task RemoveAfterAsync(
        long chatId,
        long messageId,
        DateTime time,
        CancellationToken cancellationToken)
    {
        try
        {
            await this.semaphore.WaitAsync(cancellationToken);
            this.items.Add((chatId, messageId, time));
            this.semaphore.Release();
        }
        catch (OperationCanceledException)
        {
        }
    }
}