using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServerApp.Entities;

namespace ServerApp.Database;

/// <summary>
/// Configuration of <see cref="ChatStatus"/> entity.
/// </summary>
public class ChatStatusConfiguration : IEntityTypeConfiguration<ChatStatus>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ChatStatus> builder)
    {
        builder
            .ToTable("statuses")
            .HasKey(x => x.Id);
        builder
            .Property(x => x.Id)
            .HasColumnName("id");
        builder
            .Property(x => x.ChatId)
            .HasColumnName("chat_id");
        builder
            .HasOne(x => x.Chat)
            .WithMany(x => x.Statuses)
            .HasForeignKey(x => x.ChatId);
        builder
            .Property(x => x.UserId)
            .HasColumnName("user_id");
        builder
            .HasOne(x => x.User)
            .WithMany(x => x.Statuses)
            .HasForeignKey(x => x.UserId);
        builder
            .Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired();
        builder
            .Property(x => x.Time)
            .HasColumnName("time")
            .IsRequired();
        builder
            .Property(x => x.HookId)
            .HasColumnName("hook_id")
            .IsRequired();
    }
}