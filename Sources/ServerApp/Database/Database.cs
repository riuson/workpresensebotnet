using Microsoft.EntityFrameworkCore;
using ServerApp.Entities;

namespace ServerApp.Database;

/// <summary>
/// DB Context helper.
/// </summary>
public class Database : IDatabase
{
    private readonly IServiceScope scope;
    private readonly ApplicationDbContext context;
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="Database"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider.</param>
    public Database(IServiceProvider serviceProvider)
    {
        this.scope = serviceProvider?.CreateScope() ?? throw new ArgumentNullException(nameof(serviceProvider));
        var context = this.scope.ServiceProvider.GetService<ApplicationDbContext>();
        this.context = context ?? throw new NullReferenceException("Resolved null DBContext!");
    }

    /// <inheritdoc />
    public ApplicationDbContext Context => this.context;

    /// <inheritdoc />
    public void Dispose()
    {
        this.Dispose(true);

        // Use SupressFinalize in case a subclass
        // of this type implements a finalizer.
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<int> UpdateUserStatusAsync(
        long userId,
        long chatId,
        bool isPrivateChat,
        string nickName,
        string firstName,
        string lastName,
        Status status,
        CancellationToken cancellationToken)
    {
        var user = await this.context.Users
            .FirstOrDefaultAsync(
                x => x.Id == userId,
                cancellationToken: cancellationToken);

        if (user is null)
        {
            user = new User()
            {
                Id = userId,
                FirstName = firstName,
                LastName = lastName,
                NickName = nickName,
            };
            this.context.Users.Add(user!);
        }
        else
        {
            await this.context.Entry(user).Collection(x => x.Chats).LoadAsync();
        }

        if (isPrivateChat)
        {
            // Update all registered chats for user.
            foreach (var chat in user.Chats)
            {
                await this.context.Entry(chat).Reference(x => x.Status).LoadAsync();
                chat.Status.Status = status;
                chat.Status.Time = DateTime.Now;
            }
        }
        else
        {
            // Update current chat.
            var chat = user.Chats.FirstOrDefault(x => x.ChatId == chatId);

            if (chat is null)
            {
                chat = new Chat()
                {
                    User = user,
                    ChatId = chatId,
                };
                chat.Status.Chat = chat;
                chat.Status.HookId = Guid.NewGuid();
                chat.Status.Status = status;
                chat.Status.Time = DateTime.Now;
                this.context.Chats.Add(chat);
            }
            else
            {
                await this.context.Entry(chat).Reference(x => x.Status).LoadAsync();
                chat.Status.Status = status;
                chat.Status.Time = DateTime.Now;
            }
        }

        var affectedEntities = await this.context.SaveChangesAsync(cancellationToken);
        return affectedEntities;
    }

    /// <inheritdoc />
    public async Task<(bool isSuccessfull, Status previousStatus, DateTime time)> UpdateUserStatusAsync(
        Guid hookId,
        Status status,
        CancellationToken cancellationToken)
    {
        var statusRecord = await this.context.Statuses
            .FirstOrDefaultAsync(x => x.HookId == hookId, cancellationToken);

        if (statusRecord is null)
        {
            return (false, Status.Unknown, DateTime.Now);
        }

        var previousStatus = statusRecord.Status;
        statusRecord.Status = status;
        statusRecord.Time = DateTime.Now;
        await this.context.SaveChangesAsync(cancellationToken);
        return (true, previousStatus, statusRecord.Time);
    }

    /// <inheritdoc />
    public async Task<Dictionary<long, IEnumerable<Chat>>> GetStatsAsync(
        long userId,
        long chatId,
        bool isPrivateChat,
        CancellationToken cancellationToken)
    {
        var user = await this.context.Users
            .FirstOrDefaultAsync(
                x => x.Id == userId,
                cancellationToken: cancellationToken);

        if (user is null)
        {
            return new Dictionary<long, IEnumerable<Chat>>();
        }

        await this.context.Entry(user).Collection(x => x.Chats).LoadAsync();
        var chats = isPrivateChat
            ? user.Chats.ToArray()
            : user.Chats.Where(x => x.ChatId == chatId).ToArray();

        if (chats.Length == 0)
        {
            return new Dictionary<long, IEnumerable<Chat>>();
        }

        var result = new Dictionary<long, IEnumerable<Chat>>();

        foreach (var chat in chats)
        {
            var sameChats = await this.context.Chats
                .Where(x => x.ChatId == chat.ChatId)
                .Include(x => x.User)
                .Include(x => x.Status)
                .ToArrayAsync(cancellationToken);
            result.Add(chat.ChatId, sameChats);
        }

        return result;
    }

    /// <summary>
    /// Internal dispose method.
    /// </summary>
    /// <param name="disposing">Flag of completed disposing.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposed)
        {
            if (disposing)
            {
                // Clear all property values that maybe have been set
                // when the class was instantiated
                this.context.Dispose();
                this.scope.Dispose();
            }

            // Indicate that the instance has been disposed.
            this.disposed = true;
        }
    }
}