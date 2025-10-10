using Auth.Domain.Aggregates.User;
using Auth.Domain.Constants;
using Auth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Users;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.HasKey(u => u.Id);

        b.Property(u => u.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => UserId.UnsafeFromGuid(value)
            )
            .HasColumnType("uuid")
            .IsRequired();

        b.Property(u => u.Name)
            .HasMaxLength(UserConstants.MaxNameLength)
            .HasColumnType("varchar")
            .IsRequired();

        b.Property(u => u.Email)
            .HasMaxLength(UserConstants.MaxEmailLength)
            .HasConversion(
                email => email.Value,
                value => EmailAddress.UnsafeFromString(value)
            )
            .HasColumnType("citext")
            .IsRequired();

        b.Property(u => u.Avatar)
            .HasColumnType("text")
            .IsRequired(false);

        b.Property(u => u.CreatedAt)
            .HasColumnType("timestamptz")
            .IsRequired();

        b.Property(u => u.UpdatedAt)
            .HasColumnType("timestamptz")
            .IsRequired(false);

        b.HasIndex(u => u.Email)
            .IsUnique();

        b.ToTable(t =>
        {
            t.HasCheckConstraint("chk_user_id_not_empty",
                "id != '00000000-0000-0000-0000-000000000000'");

            t.HasCheckConstraint("chk_user_email_format",
                "email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$'");

            t.HasCheckConstraint("chk_user_name_not_empty",
                "name IS NOT NULL AND length(trim(name)) > 0");
        });

        b.Navigation(u => u.EmailChangeRequests).AutoInclude(false);
        b.Navigation(u => u.RecoveryCodes).AutoInclude(false);
    }
}