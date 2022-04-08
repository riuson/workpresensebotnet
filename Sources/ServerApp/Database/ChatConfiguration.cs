using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServerApp.Entities;

namespace ServerApp.Database;

/// <summary>
/// Configuration of <see cref="Chat"/> entity.
/// </summary>
public class ChatConfiguration : IEntityTypeConfiguration<Chat>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Chat> builder)
    {
        builder
            .ToTable("chats")
            .HasKey(x => x.Id);
        builder
            .Property(x => x.Id)
            .HasColumnName("id");
        builder
            .Property(x => x.UserId)
            .HasColumnName("user_id");
        builder
            .Property(x => x.ChatId)
            .HasColumnName("chat_id");
        builder
            .HasOne(x => x.User)
            .WithMany(x => x.Chats)
            .HasForeignKey(x => x.UserId);
        builder
            .HasOne(x => x.Status)
            .WithOne(x => x.Chat)
            .HasForeignKey<ChatStatus>(x => x.ChatId);
    }
}