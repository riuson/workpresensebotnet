using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServerApp.Entities;

namespace ServerApp.Database;

/// <summary>
/// Configuration of <see cref="User"/> entity.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder
            .ToTable("users")
            .HasKey(x => x.Id);
        builder
            .Property(x => x.Id)
            .HasColumnName("id");
        builder
            .Property(x => x.FirstName)
            .HasColumnName("first_name")
            .IsRequired(false);
        builder
            .Property(x => x.LastName)
            .HasColumnName("last_name")
            .IsRequired(false);
        builder
            .Property(x => x.NickName)
            .HasColumnName("nickname")
            .IsRequired(false);
    }
}