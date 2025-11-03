using First_Sample_Project_Prompting.Models;

namespace First_Sample_Project_Prompting.Services;

/// <summary>
/// Interface for customer business logic operations.
/// </summary>
public interface ICustomerService
{
    /// <summary>
    /// Retrieves all customers.
    /// </summary>
    /// <returns>A collection of all customers.</returns>
    Task<IEnumerable<POSCustomer>> GetAllCustomersAsync();

    /// <summary>
    /// Retrieves a customer by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the customer.</param>
    /// <returns>The customer if found; otherwise, null.</returns>
    Task<POSCustomer?> GetCustomerByIdAsync(int id);

    /// <summary>
    /// Creates a new customer.
    /// </summary>
    /// <param name="viewCustomer">The customer view model containing customer data.</param>
    /// <returns>The created customer.</returns>
    Task<POSCustomer> CreateCustomerAsync(POSViewCustomer viewCustomer);

    /// <summary>
    /// Updates an existing customer.
    /// </summary>
    /// <param name="id">The unique identifier of the customer to update.</param>
    /// <param name="viewCustomer">The customer view model containing updated data.</param>
    /// <returns>The updated customer.</returns>
    Task<POSCustomer> UpdateCustomerAsync(int id, POSViewCustomer viewCustomer);

    /// <summary>
    /// Deletes a customer.
    /// </summary>
    /// <param name="id">The unique identifier of the customer to delete.</param>
    /// <returns>True if the customer was deleted; otherwise, false.</returns>
    Task<bool> DeleteCustomerAsync(int id);
}
