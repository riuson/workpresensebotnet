using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServerApp.Entities;

namespace ServerApp.Database;

/// <summary>
/// Configuration of <see cref="PinnedStatusMessage"/> entity.
/// </summary>
public class PinnedStatusesConfiguration : IEntityTypeConfiguration<PinnedStatusMessage>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PinnedStatusMessage> builder)
    {
        builder
            .ToTable("pinned_statuses")
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
            .HasOne(x => x.Chat)
            .WithMany(x => x.PinnedStatusMessages)
            .HasForeignKey(x => x.ChatId);
    }
}