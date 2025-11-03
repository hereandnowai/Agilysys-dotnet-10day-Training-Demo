using Microsoft.EntityFrameworkCore;
using First_Sample_Project_Prompting.Models;

namespace First_Sample_Project_Prompting.Data;

/// <summary>
/// Database context for the application, managing customer entities.
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
/// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
   : base(options)
    {
  }

    /// <summary>
    /// Gets or sets the DbSet for customer entities.
    /// </summary>
    public DbSet<POSCustomer> POSCustomer { get; set; } = null!;

    /// <summary>
    /// Configures the model using Fluent API.
    /// </summary>
    /// <param name="modelBuilder">The model builder instance.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure POSCustomer entity
 modelBuilder.Entity<POSCustomer>(entity =>
        {
       // Table name
            entity.ToTable("POSCustomer");

    // Primary key
            entity.HasKey(e => e.Id);

     // Configure Id as identity
   entity.Property(e => e.Id)
             .ValueGeneratedOnAdd();

// Configure Name
            entity.Property(e => e.Name)
           .IsRequired()
   .HasMaxLength(200);

    // Configure Email with unique constraint
  entity.Property(e => e.Email)
      .IsRequired()
    .HasMaxLength(255);

        entity.HasIndex(e => e.Email)
     .IsUnique()
         .HasDatabaseName("UQ_POSCustomer_Email");

            // Configure Phone
 entity.Property(e => e.Phone)
                .HasMaxLength(20);

 // Configure billing_address
        entity.Property(e => e.billing_address)
          .IsRequired()
        .HasMaxLength(500);

            // Configure CreatedAt
            entity.Property(e => e.CreatedAt)
    .IsRequired();

            // Configure UpdatedAt as nullable
       entity.Property(e => e.UpdatedAt)
       .IsRequired(false);

     // Create index on Name for better query performance
            entity.HasIndex(e => e.Name)
.HasDatabaseName("IX_POSCustomer_Name");
        });
    }

    /// <summary>
 /// Saves all changes made in this context to the database.
    /// </summary>
 /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Automatically set UpdatedAt for modified entities
        var modifiedEntities = ChangeTracker.Entries<POSCustomer>()
        .Where(e => e.State == EntityState.Modified);

        foreach (var entity in modifiedEntities)
    {
            entity.Entity.UpdatedAt = DateTime.UtcNow;
    }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
