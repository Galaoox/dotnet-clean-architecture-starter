using Company.ProjectName.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Company.ProjectName.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(product => product.Id);

        builder.Property(product => product.Id)
            .ValueGeneratedNever();

        builder.Property(product => product.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(product => product.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(product => product.CreatedAtUtc)
            .IsRequired();

        builder.Property(product => product.UpdatedAtUtc)
            .IsRequired();

        builder.HasIndex(product => product.Name)
            .IsUnique();
    }
}
