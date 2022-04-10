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
            await this.context.Entry(user).Collection(x => x.Statuses).LoadAsync();
        }

        if (isPrivateChat)
        {
            // Update all existing statuses for user.
            foreach (var userStatus in user.Statuses)
            {
                userStatus.Status = status;
                userStatus.Time = DateTime.Now;
            }
        }
        else
        {
            // Get chat record.
            var chat = await this.context.Chats.FirstOrDefaultAsync(x => x.Id == chatId, cancellationToken);

            if (chat is null)
            {
                chat = new Chat()
                {
                    Id = chatId,
                };
                this.context.Chats.Add(chat);
            }

            // Update current chat/user status.
            var chatStatus = user.Statuses.FirstOrDefault(x => x.ChatId == chatId && x.UserId == userId);

            if (chatStatus is null)
            {
                chatStatus = new ChatStatus()
                {
                    User = user,
                    Chat = chat,
                    HookId = Guid.NewGuid(),
                    Status = status,
                    Time = DateTime.Now,
                };
                this.context.Statuses.Add(chatStatus);
            }
            else
            {
                chatStatus.Status = status;
                chatStatus.Time = DateTime.Now;
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
    public async Task<Dictionary<long, IEnumerable<ChatStatus>>> GetStatsAsync(
        long userId,
        long chatId,
        bool isPrivateChat,
        CancellationToken cancellationToken)
    {
        var user = await this.context.Users
            .FirstOrDefaultAsync(
                x => x.Id == userId,
                cancellationToken: cancellationToken);

        var selectedChats = new List<Chat>();

        if (user is null)
        {
            return new Dictionary<long, IEnumerable<ChatStatus>>();
        }

        if (isPrivateChat)
        {
            await this.context.Entry(user)
                .Collection(x => x.Statuses)
                .LoadAsync(cancellationToken);

            foreach (var userStatus in user.Statuses)
            {
                await this.context.Entry(userStatus)
                    .Reference(x => x.Chat)
                    .LoadAsync(cancellationToken);

                if (userStatus.Chat != null)
                {
                    selectedChats.Add(userStatus.Chat);
                }
            }
        }
        else
        {
            var chat = await this.context.Chats
                .FirstOrDefaultAsync(x => x.Id == chatId, cancellationToken);

            if (chat != null)
            {
                selectedChats.Add(chat);
            }
        }

        if (selectedChats.Count == 0)
        {
            return new Dictionary<long, IEnumerable<ChatStatus>>();
        }

        var result = new Dictionary<long, IEnumerable<ChatStatus>>();

        foreach (var selectedChat in selectedChats)
        {
            await this.context.Entry(selectedChat)
                .Collection(x => x.Statuses)
                .LoadAsync(cancellationToken);

            foreach (var selectedChatStatus in selectedChat.Statuses)
            {
                await this.context.Entry(selectedChatStatus)
                    .Reference(x => x.User)
                    .LoadAsync(cancellationToken);
            }

            result.Add(selectedChat.Id, selectedChat.Statuses.ToArray());
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<Dictionary<long, Guid>> GetHooksAsync(
        long userId,
        CancellationToken cancellationToken)
    {
        var chats = await this.context.Statuses
            .Where(x => x.UserId == userId)
            .Include(x => x.Chat)
            .ToListAsync(cancellationToken);
        return chats.ToDictionary(x => x.ChatId, x => x.HookId);
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