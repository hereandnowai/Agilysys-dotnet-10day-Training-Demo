using Microsoft.AspNetCore.Mvc;
using First_Sample_Project_Prompting.Models;
using First_Sample_Project_Prompting.Services;

namespace First_Sample_Project_Prompting.Controllers;

/// <summary>
/// Controller for managing customer operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomersController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomersController"/> class.
    /// </summary>
    /// <param name="customerService">The customer service.</param>
    /// <param name="logger">The logger instance.</param>
    public CustomersController(ICustomerService customerService, ILogger<CustomersController> logger)
    {
        _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
 _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves a list of all customers.
    /// </summary>
    /// <returns>A list of customers.</returns>
    /// <response code="200">Returns the list of customers.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<POSCustomer>>> GetAllCustomers()
    {
  try
      {
      _logger.LogInformation("GET /api/customers - Retrieving all customers");
    var customers = await _customerService.GetAllCustomersAsync();
return Ok(customers);
   }
        catch (Exception ex)
   {
       _logger.LogError(ex, "Error retrieving all customers");
     return StatusCode(500, "An error occurred while retrieving customers");
        }
    }

    /// <summary>
/// Retrieves a specific customer by ID.
    /// </summary>
    /// <param name="id">The customer ID.</param>
    /// <returns>The customer if found.</returns>
    /// <response code="200">Returns the customer.</response>
    /// <response code="400">If the ID is invalid.</response>
  /// <response code="404">If the customer is not found.</response>
/// <response code="500">If an internal server error occurs.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<POSCustomer>> GetCustomerById(int id)
    {
        try
{
       _logger.LogInformation("GET /api/customers/{Id} - Retrieving customer", id);
      
            var customer = await _customerService.GetCustomerByIdAsync(id);
        
  if (customer == null)
  {
      _logger.LogWarning("Customer not found: {Id}", id);
return NotFound($"Customer with ID {id} not found");
            }

   return Ok(customer);
  }
      catch (ArgumentException ex)
        {
    _logger.LogWarning(ex, "Invalid customer ID: {Id}", id);
       return BadRequest(ex.Message);
  }
      catch (Exception ex)
   {
     _logger.LogError(ex, "Error retrieving customer: {Id}", id);
 return StatusCode(500, "An error occurred while retrieving the customer");
        }
    }

    /// <summary>
    /// Creates a new customer.
    /// </summary>
    /// <param name="viewCustomer">The customer data.</param>
 /// <returns>The created customer.</returns>
    /// <response code="201">Returns the newly created customer.</response>
    /// <response code="400">If the customer data is invalid.</response>
    /// <response code="409">If a customer with the same email already exists.</response>
    /// <response code="500">If an internal server error occurs.</response>
  [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<POSCustomer>> CreateCustomer([FromBody] POSViewCustomer viewCustomer)
    {
        try
     {
       _logger.LogInformation("POST /api/customers - Creating new customer");

   // Validate model state
   if (!ModelState.IsValid)
       {
  return BadRequest(ModelState);
       }

     var customer = await _customerService.CreateCustomerAsync(viewCustomer);
    
   _logger.LogInformation("Customer created successfully: {Id}", customer.Id);
   
 return CreatedAtAction(
        nameof(GetCustomerById),
     new { id = customer.Id },
        customer);
        }
    catch (ArgumentException ex)
   {
        _logger.LogWarning(ex, "Validation error while creating customer");
  return BadRequest(ex.Message);
  }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Duplicate email error while creating customer");
   return Conflict(ex.Message);
        }
 catch (Exception ex)
      {
    _logger.LogError(ex, "Error creating customer");
 return StatusCode(500, "An error occurred while creating the customer");
    }
    }

    /// <summary>
    /// Updates an existing customer.
  /// </summary>
    /// <param name="id">The customer ID.</param>
    /// <param name="viewCustomer">The updated customer data.</param>
    /// <returns>The updated customer.</returns>
    /// <response code="200">Returns the updated customer.</response>
    /// <response code="400">If the customer data is invalid.</response>
    /// <response code="404">If the customer is not found.</response>
    /// <response code="409">If a customer with the same email already exists.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<POSCustomer>> UpdateCustomer(int id, [FromBody] POSViewCustomer viewCustomer)
    {
        try
    {
    _logger.LogInformation("PUT /api/customers/{Id} - Updating customer", id);

       // Validate model state
     if (!ModelState.IsValid)
  {
    return BadRequest(ModelState);
}

       var customer = await _customerService.UpdateCustomerAsync(id, viewCustomer);
         
 _logger.LogInformation("Customer updated successfully: {Id}", id);

            return Ok(customer);
        }
        catch (ArgumentException ex)
    {
     _logger.LogWarning(ex, "Validation error while updating customer: {Id}", id);
  return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
 _logger.LogWarning(ex, "Customer not found for update: {Id}", id);
     return NotFound(ex.Message);
        }
  catch (InvalidOperationException ex)
        {
     _logger.LogWarning(ex, "Duplicate email error while updating customer: {Id}", id);
          return Conflict(ex.Message);
 }
        catch (Exception ex)
   {
       _logger.LogError(ex, "Error updating customer: {Id}", id);
            return StatusCode(500, "An error occurred while updating the customer");
    }
    }

    /// <summary>
 /// Deletes a customer by ID.
    /// </summary>
    /// <param name="id">The customer ID.</param>
    /// <returns>No content with deleted customer ID.</returns>
 /// <response code="204">If the customer was successfully deleted.</response>
    /// <response code="400">If the ID is invalid.</response>
    /// <response code="404">If the customer is not found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
   try
        {
    _logger.LogInformation("DELETE /api/customers/{Id} - Deleting customer", id);

       var result = await _customerService.DeleteCustomerAsync(id);
         
   if (!result)
 {
   _logger.LogWarning("Customer not found for deletion: {Id}", id);
 return NotFound($"Customer with ID {id} not found");
        }

 _logger.LogInformation("Customer deleted successfully: {Id}", id);
            return NoContent();
     }
        catch (ArgumentException ex)
      {
    _logger.LogWarning(ex, "Invalid customer ID for deletion: {Id}", id);
     return BadRequest(ex.Message);
    }
      catch (Exception ex)
        {
   _logger.LogError(ex, "Error deleting customer: {Id}", id);
    return StatusCode(500, "An error occurred while deleting the customer");
        }
    }
}
