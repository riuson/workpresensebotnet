using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServerApp.Entities;

namespace ServerApp.Database;

/// <summary>
/// Configuration of <see cref="PhoneNumber"/> entity.
/// </summary>
public class PhoneNumberConfiguration : IEntityTypeConfiguration<PhoneNumber>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PhoneNumber> builder)
    {
        builder
            .ToTable("phone_numbers")
            .HasKey(x => x.Id);
        builder
            .Property(x => x.Id)
            .HasColumnName("id");
        builder
            .Property(x => x.Number)
            .HasColumnName("number")
            .IsRequired();
        builder
            .Property(x => x.UserId)
            .HasColumnName("user_id");
        builder
            .HasOne(x => x.User)
            .WithMany(x => x.PhoneNumbers)
            .HasForeignKey(x => x.UserId);
    }
}