using Microsoft.EntityFrameworkCore;
using Products.Models;

namespace Products.Data;

/// <summary>
/// Application database context for managing product data.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the products DbSet.
    /// </summary>
    public DbSet<Product> Products { get; set; }

    /// <summary>
    /// Configures the model using Fluent API.
    /// </summary>
    /// <param name="modelBuilder">The model builder instance.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            // Configure table name
            entity.ToTable("Products");

            // Configure primary key
            entity.HasKey(e => e.ProductId);

            // Configure indexes for better query performance
            entity.HasIndex(e => e.SKU)
                .IsUnique()
                .HasDatabaseName("IX_Products_SKU");

            entity.HasIndex(e => e.ProductName)
                .HasDatabaseName("IX_Products_ProductName");

            // Configure properties
            entity.Property(e => e.ProductName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .HasMaxLength(1000);

            entity.Property(e => e.UnitPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            entity.Property(e => e.SKU)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Quantity)
                .IsRequired();

            entity.Property(e => e.CreatedOn)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.ModifiedOn)
                .IsRequired(false);

            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(100);
        });
    }
}
