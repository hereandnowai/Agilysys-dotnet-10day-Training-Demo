using First_Sample_Project_Prompting.Models;

namespace First_Sample_Project_Prompting.Repositories;

/// <summary>
/// Interface for customer repository operations.
/// </summary>
public interface ICustomerRepository
{
    /// <summary>
    /// Retrieves all customers from the database.
    /// </summary>
    /// <returns>A collection of all customers.</returns>
    Task<IEnumerable<POSCustomer>> GetAllAsync();

    /// <summary>
    /// Retrieves a customer by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the customer.</param>
    /// <returns>The customer if found; otherwise, null.</returns>
    Task<POSCustomer?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieves a customer by their email address.
    /// </summary>
    /// <param name="email">The email address of the customer.</param>
    /// <returns>The customer if found; otherwise, null.</returns>
    Task<POSCustomer?> GetByEmailAsync(string email);

    /// <summary>
    /// Creates a new customer in the database.
    /// </summary>
    /// <param name="customer">The customer entity to create.</param>
    /// <returns>The created customer with generated ID.</returns>
    Task<POSCustomer> CreateAsync(POSCustomer customer);

    /// <summary>
    /// Updates an existing customer in the database.
    /// </summary>
    /// <param name="customer">The customer entity to update.</param>
    /// <returns>The updated customer.</returns>
    Task<POSCustomer> UpdateAsync(POSCustomer customer);

    /// <summary>
    /// Deletes a customer from the database.
    /// </summary>
    /// <param name="id">The unique identifier of the customer to delete.</param>
    /// <returns>True if the customer was deleted; otherwise, false.</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Checks if a customer with the specified email exists.
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <param name="excludeId">Optional customer ID to exclude from the check (for updates).</param>
    /// <returns>True if the email exists; otherwise, false.</returns>
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);
}
