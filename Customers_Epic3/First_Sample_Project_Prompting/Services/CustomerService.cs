using First_Sample_Project_Prompting.Models;
using First_Sample_Project_Prompting.Repositories;

namespace First_Sample_Project_Prompting.Services;

/// <summary>
/// Service for managing customer business logic operations.
/// </summary>
public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;
    private readonly ILogger<CustomerService> _logger;

/// <summary>
    /// Initializes a new instance of the <see cref="CustomerService"/> class.
    /// </summary>
    /// <param name="repository">The customer repository.</param>
    /// <param name="logger">The logger instance.</param>
    public CustomerService(ICustomerRepository repository, ILogger<CustomerService> logger)
  {
    _repository = repository ?? throw new ArgumentNullException(nameof(repository));
   _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all customers.
  /// </summary>
    /// <returns>A collection of all customers.</returns>
    public async Task<IEnumerable<POSCustomer>> GetAllCustomersAsync()
    {
        try
        {
      _logger.LogInformation("Fetching all customers");
            return await _repository.GetAllAsync();
      }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all customers");
            throw;
        }
    }

    /// <summary>
    /// Retrieves a customer by their unique identifier.
    /// </summary>
  /// <param name="id">The unique identifier of the customer.</param>
    /// <returns>The customer if found; otherwise, null.</returns>
    public async Task<POSCustomer?> GetCustomerByIdAsync(int id)
    {
    try
  {
      // Validate customer ID
      if (id <= 0)
 {
         _logger.LogWarning("Invalid customer ID provided: {Id}", id);
     throw new ArgumentException("Customer ID must be greater than zero", nameof(id));
  }

    _logger.LogInformation("Fetching customer with ID: {Id}", id);
            return await _repository.GetByIdAsync(id);
      }
        catch (ArgumentException)
      {
     throw;
   }
    catch (Exception ex)
        {
   _logger.LogError(ex, "Error occurred while retrieving customer with ID: {Id}", id);
  throw;
        }
    }

    /// <summary>
    /// Creates a new customer.
    /// </summary>
    /// <param name="viewCustomer">The customer view model containing customer data.</param>
    /// <returns>The created customer.</returns>
public async Task<POSCustomer> CreateCustomerAsync(POSViewCustomer viewCustomer)
    {
        try
     {
      // Validate input
if (viewCustomer == null)
            {
 throw new ArgumentNullException(nameof(viewCustomer), "Customer data cannot be null");
            }

   _logger.LogInformation("Creating new customer with email: {Email}", viewCustomer.Email);

     // Perform custom validation
     viewCustomer.ValidateInput();

        // Check if email already exists
            var emailExists = await _repository.EmailExistsAsync(viewCustomer.Email);
          if (emailExists)
      {
   _logger.LogWarning("Duplicate email detected: {Email}", viewCustomer.Email);
         throw new InvalidOperationException($"A customer with email '{viewCustomer.Email}' already exists");
     }

            // Convert view model to entity
            var customer = viewCustomer.ToEntity();

      // Create customer in database
 var createdCustomer = await _repository.CreateAsync(customer);

 _logger.LogInformation("Customer created successfully with ID: {Id}", createdCustomer.Id);
return createdCustomer;
        }
 catch (ArgumentException)
        {
       throw;
        }
        catch (InvalidOperationException)
        {
 throw;
      }
    catch (Exception ex)
        {
      _logger.LogError(ex, "Error occurred while creating customer");
         throw;
    }
    }

    /// <summary>
    /// Updates an existing customer.
    /// </summary>
    /// <param name="id">The unique identifier of the customer to update.</param>
    /// <param name="viewCustomer">The customer view model containing updated data.</param>
  /// <returns>The updated customer.</returns>
    public async Task<POSCustomer> UpdateCustomerAsync(int id, POSViewCustomer viewCustomer)
    {
   try
        {
            // Validate inputs
            if (id <= 0)
 {
  throw new ArgumentException("Customer ID must be greater than zero", nameof(id));
            }

  if (viewCustomer == null)
    {
throw new ArgumentNullException(nameof(viewCustomer), "Customer data cannot be null");
          }

       _logger.LogInformation("Updating customer with ID: {Id}", id);

  // Perform custom validation
        viewCustomer.ValidateInput();

      // Check if customer exists
       var existingCustomer = await _repository.GetByIdAsync(id);
     if (existingCustomer == null)
{
          _logger.LogWarning("Customer not found for update: {Id}", id);
      throw new KeyNotFoundException($"Customer with ID {id} not found");
       }

            // Check if email is being changed and if new email already exists
        var emailExists = await _repository.EmailExistsAsync(viewCustomer.Email, id);
         if (emailExists)
        {
                _logger.LogWarning("Duplicate email detected during update: {Email}", viewCustomer.Email);
      throw new InvalidOperationException($"A customer with email '{viewCustomer.Email}' already exists");
        }

            // Update entity with new values
       viewCustomer.UpdateEntity(existingCustomer);

       // Save changes
      var updatedCustomer = await _repository.UpdateAsync(existingCustomer);

         _logger.LogInformation("Customer updated successfully: {Id}", id);
 return updatedCustomer;
        }
      catch (ArgumentException)
        {
  throw;
      }
   catch (KeyNotFoundException)
   {
          throw;
    }
        catch (InvalidOperationException)
        {
    throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating customer with ID: {Id}", id);
            throw;
 }
    }

    /// <summary>
    /// Deletes a customer.
    /// </summary>
    /// <param name="id">The unique identifier of the customer to delete.</param>
    /// <returns>True if the customer was deleted; otherwise, false.</returns>
    public async Task<bool> DeleteCustomerAsync(int id)
  {
  try
     {
            // Validate customer ID
if (id <= 0)
     {
     throw new ArgumentException("Customer ID must be greater than zero", nameof(id));
          }

            _logger.LogInformation("Deleting customer with ID: {Id}", id);

 // Check if customer exists
    var existingCustomer = await _repository.GetByIdAsync(id);
     if (existingCustomer == null)
          {
   _logger.LogWarning("Customer not found for deletion: {Id}", id);
     return false;
  }

   // Delete customer
  var result = await _repository.DeleteAsync(id);

    if (result)
            {
    _logger.LogInformation("Customer deleted successfully: {Id}", id);
            }

       return result;
  }
      catch (ArgumentException)
     {
       throw;
     }
        catch (Exception ex)
 {
  _logger.LogError(ex, "Error occurred while deleting customer with ID: {Id}", id);
        throw;
        }
    }
}
