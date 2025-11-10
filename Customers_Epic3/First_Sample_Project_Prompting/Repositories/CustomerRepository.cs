using Microsoft.EntityFrameworkCore;
using First_Sample_Project_Prompting.Data;
using First_Sample_Project_Prompting.Models;

namespace First_Sample_Project_Prompting.Repositories;

/// <summary>
/// Repository for managing customer data access operations.
/// </summary>
public class CustomerRepository : ICustomerRepository
{
 private readonly ApplicationDbContext _context;
    private readonly ILogger<CustomerRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerRepository"/> class.
    /// </summary>
/// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public CustomerRepository(ApplicationDbContext context, ILogger<CustomerRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all customers from the database.
    /// </summary>
    /// <returns>A collection of all customers.</returns>
 public async Task<IEnumerable<POSCustomer>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all customers from database");
        return await _context.POSCustomer
     .AsNoTracking()
            .OrderBy(c => c.Name)
     .ToListAsync();
 }

    /// <summary>
    /// Retrieves a customer by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the customer.</param>
    /// <returns>The customer if found; otherwise, null.</returns>
    public async Task<POSCustomer?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving customer with ID: {Id}", id);
        return await _context.POSCustomer
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    /// <summary>
    /// Retrieves a customer by their email address.
    /// </summary>
    /// <param name="email">The email address of the customer.</param>
    /// <returns>The customer if found; otherwise, null.</returns>
    public async Task<POSCustomer?> GetByEmailAsync(string email)
    {
    if (string.IsNullOrWhiteSpace(email))
       return null;

        _logger.LogInformation("Retrieving customer with email: {Email}", email);
        var normalizedEmail = email.Trim().ToLowerInvariant();
      
        return await _context.POSCustomer
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Email.ToLower() == normalizedEmail);
    }

    /// <summary>
    /// Creates a new customer in the database.
    /// </summary>
    /// <param name="customer">The customer entity to create.</param>
    /// <returns>The created customer with generated ID.</returns>
    public async Task<POSCustomer> CreateAsync(POSCustomer customer)
    {
        if (customer == null)
  throw new ArgumentNullException(nameof(customer));

        _logger.LogInformation("Creating new customer: {Email}", customer.Email);
        
  // Set creation timestamp
      customer.CreatedAt = DateTime.UtcNow;
        customer.UpdatedAt = null;

        await _context.POSCustomer.AddAsync(customer);
        await _context.SaveChangesAsync();

  _logger.LogInformation("Customer created successfully with ID: {Id}", customer.Id);
    return customer;
    }

    /// <summary>
  /// Updates an existing customer in the database.
    /// </summary>
    /// <param name="customer">The customer entity to update.</param>
    /// <returns>The updated customer.</returns>
    public async Task<POSCustomer> UpdateAsync(POSCustomer customer)
    {
    if (customer == null)
        throw new ArgumentNullException(nameof(customer));

        _logger.LogInformation("Updating customer with ID: {Id}", customer.Id);

     // Set update timestamp
        customer.UpdatedAt = DateTime.UtcNow;

        _context.POSCustomer.Update(customer);
        await _context.SaveChangesAsync();

    _logger.LogInformation("Customer updated successfully: {Id}", customer.Id);
        return customer;
    }

    /// <summary>
    /// Deletes a customer from the database.
    /// </summary>
    /// <param name="id">The unique identifier of the customer to delete.</param>
    /// <returns>True if the customer was deleted; otherwise, false.</returns>
    public async Task<bool> DeleteAsync(int id)
    {
   _logger.LogInformation("Deleting customer with ID: {Id}", id);

        var customer = await _context.POSCustomer.FindAsync(id);
        
  if (customer == null)
   {
     _logger.LogWarning("Customer not found for deletion: {Id}", id);
  return false;
        }

        _context.POSCustomer.Remove(customer);
 await _context.SaveChangesAsync();

  _logger.LogInformation("Customer deleted successfully: {Id}", id);
        return true;
    }

    /// <summary>
    /// Checks if a customer with the specified email exists.
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <param name="excludeId">Optional customer ID to exclude from the check.</param>
    /// <returns>True if the email exists; otherwise, false.</returns>
    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
  {
  if (string.IsNullOrWhiteSpace(email))
            return false;

        var normalizedEmail = email.Trim().ToLowerInvariant();
        
        var query = _context.POSCustomer.AsNoTracking()
            .Where(c => c.Email.ToLower() == normalizedEmail);

     // Exclude specific customer ID if provided (for update scenarios)
        if (excludeId.HasValue)
        {
          query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }
}
