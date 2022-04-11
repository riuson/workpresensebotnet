using Microsoft.EntityFrameworkCore;
using ServerApp.Entities;

namespace ServerApp.Database;

/// <summary>
/// DB Context helper.
/// </summary>
public class Database : IDatabase
{
    private readonly ApplicationDbContext context;

    /// <summary>
    /// Initializes a new instance of the <see cref="Database"/> class.
    /// </summary>
    /// <param name="context">Application's database context.</param>
    public Database(ApplicationDbContext context)
    {
        this.context = context ?? throw new NullReferenceException("Resolved null DBContext!");
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

        if (isPrivateChat && user is not null)
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
        else if (!isPrivateChat)
        {
            var chat = await this.context.Chats
                .FirstOrDefaultAsync(x => x.Id == chatId, cancellationToken);

            if (chat != null)
            {
                selectedChats.Add(chat);
            }
        }
        else
        {
            return new Dictionary<long, IEnumerable<ChatStatus>>();
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

    /// <inheritdoc />
    public async Task<(bool isSuccessfull, long messageId, DateTime time)> GetPinnedMessageAsync(
        long chatId,
        MessageType messageType,
        CancellationToken cancellationToken)
    {
        var result = await this.context.PinnedMessages
            .FirstOrDefaultAsync(
                x => x.ChatId == chatId && x.MessageType == messageType,
                cancellationToken);

        if (result is null)
        {
            return (false, default, default);
        }

        return (true, result.MessageId, result.Time);
    }

    /// <inheritdoc />
    public async Task UpdatePinnedMessageAsync(
        long chatId,
        int messageId,
        MessageType messageType,
        DateTime time,
        CancellationToken cancellationToken)
    {
        var chat = await this.context.Chats
            .FirstOrDefaultAsync(
                x => x.Id == chatId,
                cancellationToken);

        if (chat is null)
        {
            chat = new Chat()
            {
                Id = chatId,
            };
            this.context.Chats.Add(chat);
        }

        var pinnedMessage = await this.context.PinnedMessages
            .FirstOrDefaultAsync(
                x => x.ChatId == chatId && x.MessageType == messageType,
                cancellationToken);

        if (pinnedMessage is null)
        {
            pinnedMessage = new PinnedMessage()
            {
                Chat = chat,
                MessageId = messageId,
                Time = time,
                MessageType = messageType,
            };
            this.context.PinnedMessages.Add(pinnedMessage);
        }

        await this.context.SaveChangesAsync(cancellationToken);
    }
}