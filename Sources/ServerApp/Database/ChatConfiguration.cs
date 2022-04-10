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
    }
}