using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServerApp.Entities;

namespace ServerApp.Database;

/// <summary>
/// Configuration of <see cref="PinnedMessage"/> entity.
/// </summary>
public class PinnedMessageConfiguration : IEntityTypeConfiguration<PinnedMessage>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PinnedMessage> builder)
    {
        builder
            .ToTable("pinned_messages")
            .HasKey(x => x.Id);
        builder
            .Property(x => x.Id)
            .HasColumnName("id");
        builder
            .Property(x => x.ChatId)
            .HasColumnName("chat_id")
            .IsRequired();
        builder
            .Property(x => x.MessageId)
            .HasColumnName("message_id")
            .IsRequired();
        builder
            .Property(x => x.Time)
            .HasColumnName("time")
            .IsRequired();
        builder
            .Property(x => x.MessageType)
            .HasColumnName("message_type")
            .IsRequired();
        builder
            .HasOne(x => x.Chat)
            .WithMany(x => x.PinnedStatusMessages)
            .HasForeignKey(x => x.ChatId);
    }
}