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
    public async Task<IEnumerable<long>> UpdateUserStatusAsync(
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
        var result = new List<long>();

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
            await this.context.Entry(user)
                .Collection(x => x.Statuses)
                .LoadAsync(cancellationToken);
        }

        if (isPrivateChat)
        {
            // Update all existing statuses for user.
            foreach (var userStatus in user.Statuses)
            {
                userStatus.Status = status;
                userStatus.Time = DateTime.Now;
                result.Add(userStatus.ChatId);
            }
        }
        else
        {
            // Get chat record.
            var chat = await this.context.Chats
                .FirstOrDefaultAsync(x => x.Id == chatId, cancellationToken);

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

            result.Add(chatId);
        }

        await this.context.SaveChangesAsync(cancellationToken);
        return result;
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
    public async Task<IEnumerable<Chat>> GetStatsAsync(
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
            await this.context.Users
                .Where(x => x.Id == userId)
                .Include(x => x.Statuses)
                .ThenInclude(x => x.Chat)
                .LoadAsync(cancellationToken);

            selectedChats.AddRange(
                user.Statuses
                    .Where(x => x.Chat is not null)
                    .Select(x => x.Chat!)
                    .Distinct());
        }
        else if (!isPrivateChat)
        {
            var chat = await this.context.Chats
                .Where(x => x.Id == chatId)
                .Include(x => x.Statuses)
                .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(cancellationToken);

            if (chat is not null)
            {
                selectedChats.Add(chat);
            }
        }
        else
        {
            return new Chat[] { };
        }

        return selectedChats;
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
        else
        {
            pinnedMessage.Time = time;
            pinnedMessage.MessageId = messageId;
        }

        await this.context.SaveChangesAsync(cancellationToken);
    }
}