using Company.ProjectName.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Company.ProjectName.Infrastructure.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(token => token.Id);

        builder.Property(token => token.TokenHash)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(token => token.FamilyId)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(token => token.ReplacedByTokenHash)
            .HasMaxLength(128);

        builder.HasIndex(token => token.TokenHash)
            .IsUnique();

        builder.HasIndex(token => new { token.UserId, token.FamilyId });

        builder.HasOne(token => token.User)
            .WithMany()
            .HasForeignKey(token => token.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
