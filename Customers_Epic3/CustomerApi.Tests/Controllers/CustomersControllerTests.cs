using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using First_Sample_Project_Prompting.Controllers;
using First_Sample_Project_Prompting.Models;
using First_Sample_Project_Prompting.Services;

namespace CustomerApi.Tests.Controllers;

/// <summary>
/// Unit tests for CustomersController class using XUnit framework.
/// </summary>
public class CustomersControllerTests
{
    private readonly Mock<ICustomerService> _mockService;
    private readonly Mock<ILogger<CustomersController>> _mockLogger;
    private readonly CustomersController _controller;

    /// <summary>
    /// Initializes a new instance of the test class.
    /// Constructor is called before each test method (XUnit behavior).
    /// </summary>
public CustomersControllerTests()
    {
        _mockService = new Mock<ICustomerService>();
        _mockLogger = new Mock<ILogger<CustomersController>>();
    _controller = new CustomersController(_mockService.Object, _mockLogger.Object);
    }

    #region GetAllCustomers Tests

    /// <summary>
    /// Tests that GetAllCustomers returns OK with customer list.
    /// </summary>
    [Fact]
    public async Task GetAllCustomers_ReturnsOkResult_WithCustomerList()
    {
        // Arrange
   var customers = new List<POSCustomer>
  {
    new() { Id = 1, Name = "John Doe", Email = "john@test.com", billing_address = "123 Main St" },
            new() { Id = 2, Name = "Jane Smith", Email = "jane@test.com", billing_address = "456 Oak Ave" }
        };
        _mockService.Setup(s => s.GetAllCustomersAsync()).ReturnsAsync(customers);

        // Act
   var result = await _controller.GetAllCustomers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
  var returnedCustomers = Assert.IsAssignableFrom<IEnumerable<POSCustomer>>(okResult.Value);
        Assert.Equal(2, returnedCustomers.Count());
        _mockService.Verify(s => s.GetAllCustomersAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that GetAllCustomers returns OK with empty list when no customers exist.
    /// </summary>
  [Fact]
    public async Task GetAllCustomers_ReturnsOkResult_WithEmptyList_WhenNoCustomers()
    {
     // Arrange
 _mockService.Setup(s => s.GetAllCustomersAsync()).ReturnsAsync(new List<POSCustomer>());

// Act
        var result = await _controller.GetAllCustomers();

        // Assert
     var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedCustomers = Assert.IsAssignableFrom<IEnumerable<POSCustomer>>(okResult.Value);
        Assert.Empty(returnedCustomers);
    }

    /// <summary>
    /// Tests that GetAllCustomers returns 500 on exception.
    /// </summary>
    [Fact]
 public async Task GetAllCustomers_Returns500_OnException()
    {
        // Arrange
        _mockService.Setup(s => s.GetAllCustomersAsync()).ThrowsAsync(new Exception("Test exception"));

        // Act
   var result = await _controller.GetAllCustomers();

   // Assert
    var objectResult = Assert.IsType<ObjectResult>(result.Result);
  Assert.Equal(500, objectResult.StatusCode);
    }

    #endregion

  #region GetCustomerById Tests

    /// <summary>
    /// Tests that GetCustomerById returns OK when customer exists.
    /// </summary>
    [Fact]
    public async Task GetCustomerById_ReturnsOkResult_WhenCustomerExists()
    {
  // Arrange
        var id = 1;
        var customer = new POSCustomer 
        { 
     Id = id, 
       Name = "John Doe", 
   Email = "john@test.com",
        billing_address = "123 Main St"
        };
   _mockService.Setup(s => s.GetCustomerByIdAsync(id)).ReturnsAsync(customer);

        // Act
   var result = await _controller.GetCustomerById(id);

      // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedCustomer = Assert.IsType<POSCustomer>(okResult.Value);
     Assert.Equal(id, returnedCustomer.Id);
        Assert.Equal("John Doe", returnedCustomer.Name);
    }

    /// <summary>
    /// Tests that GetCustomerById returns NotFound when customer doesn't exist.
    /// </summary>
    [Fact]
    public async Task GetCustomerById_ReturnsNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        var id = 999;
        _mockService.Setup(s => s.GetCustomerByIdAsync(id)).ReturnsAsync((POSCustomer?)null);

        // Act
     var result = await _controller.GetCustomerById(id);

    // Assert
   var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
  Assert.Contains($"Customer with ID {id} not found", notFoundResult.Value?.ToString());
    }

    /// <summary>
    /// Tests that GetCustomerById returns BadRequest for invalid ID.
    /// </summary>
    [Fact]
    public async Task GetCustomerById_ReturnsBadRequest_ForInvalidId()
    {
   // Arrange
        var invalidId = 0;
        _mockService.Setup(s => s.GetCustomerByIdAsync(invalidId))
       .ThrowsAsync(new ArgumentException("Invalid ID"));

        // Act
        var result = await _controller.GetCustomerById(invalidId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
   Assert.NotNull(badRequestResult.Value);
    }

    #endregion

    #region CreateCustomer Tests

    /// <summary>
    /// Tests that CreateCustomer returns CreatedAtAction with created customer.
    /// </summary>
    [Fact]
    public async Task CreateCustomer_ReturnsCreatedAtAction_WithCreatedCustomer()
    {
        // Arrange
  var viewCustomer = new POSViewCustomer
        {
         Name = "John Doe",
   Email = "john@test.com",
        Phone = "555-0100",
       BillingAddress = "123 Main St"
        };
      
      var createdCustomer = new POSCustomer
   {
    Id = 1,
   Name = "John Doe",
     Email = "john@test.com",
       Phone = "555-0100",
   billing_address = "123 Main St"
        };

        _mockService.Setup(s => s.CreateCustomerAsync(viewCustomer)).ReturnsAsync(createdCustomer);

 // Act
   var result = await _controller.CreateCustomer(viewCustomer);

   // Assert
   var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
      Assert.Equal(nameof(CustomersController.GetCustomerById), createdResult.ActionName);
        Assert.Equal(1, createdResult.RouteValues?["id"]);
        var returnedCustomer = Assert.IsType<POSCustomer>(createdResult.Value);
Assert.Equal(1, returnedCustomer.Id);
    }

    /// <summary>
    /// Tests that CreateCustomer returns BadRequest for validation error.
 /// </summary>
    [Fact]
    public async Task CreateCustomer_ReturnsBadRequest_ForValidationError()
    {
        // Arrange
        var viewCustomer = new POSViewCustomer
   {
            Name = "John Doe",
Email = "invalid-email",
BillingAddress = "123 Main St"
        };
   
        _mockService.Setup(s => s.CreateCustomerAsync(viewCustomer))
     .ThrowsAsync(new ArgumentException("Validation failed"));

   // Act
        var result = await _controller.CreateCustomer(viewCustomer);

        // Assert
     var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Contains("Validation failed", badRequestResult.Value?.ToString());
    }

    /// <summary>
    /// Tests that CreateCustomer returns Conflict when email already exists.
    /// </summary>
    [Fact]
    public async Task CreateCustomer_ReturnsConflict_WhenEmailExists()
    {
    // Arrange
   var viewCustomer = new POSViewCustomer
   {
Name = "John Doe",
  Email = "john@test.com",
      BillingAddress = "123 Main St"
    };
        
_mockService.Setup(s => s.CreateCustomerAsync(viewCustomer))
            .ThrowsAsync(new InvalidOperationException("Email already exists"));

    // Act
     var result = await _controller.CreateCustomer(viewCustomer);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.Contains("Email already exists", conflictResult.Value?.ToString());
    }

/// <summary>
    /// Tests that CreateCustomer handles null input gracefully.
 /// </summary>
    [Fact]
    public async Task CreateCustomer_ReturnsBadRequest_WhenInputIsNull()
    {
  // Arrange
    _mockService.Setup(s => s.CreateCustomerAsync(null!))
       .ThrowsAsync(new ArgumentNullException("viewCustomer"));

        // Act
        var result = await _controller.CreateCustomer(null!);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    #endregion

 #region UpdateCustomer Tests

    /// <summary>
    /// Tests that UpdateCustomer returns OK with updated customer.
    /// </summary>
    [Fact]
    public async Task UpdateCustomer_ReturnsOkResult_WithUpdatedCustomer()
    {
        // Arrange
        var id = 1;
   var viewCustomer = new POSViewCustomer
   {
            Name = "John Updated",
      Email = "john@test.com",
       BillingAddress = "123 Main St"
    };

   var updatedCustomer = new POSCustomer
   {
    Id = id,
   Name = "John Updated",
    Email = "john@test.com",
     billing_address = "123 Main St"
     };

        _mockService.Setup(s => s.UpdateCustomerAsync(id, viewCustomer))
            .ReturnsAsync(updatedCustomer);

// Act
        var result = await _controller.UpdateCustomer(id, viewCustomer);

  // Assert
   var okResult = Assert.IsType<OkObjectResult>(result.Result);
   var returnedCustomer = Assert.IsType<POSCustomer>(okResult.Value);
        Assert.Equal("John Updated", returnedCustomer.Name);
    }

    /// <summary>
    /// Tests that UpdateCustomer returns NotFound when customer doesn't exist.
    /// </summary>
    [Fact]
    public async Task UpdateCustomer_ReturnsNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        var id = 999;
        var viewCustomer = new POSViewCustomer
  {
       Name = "John Doe",
            Email = "john@test.com",
  BillingAddress = "123 Main St"
        };

        _mockService.Setup(s => s.UpdateCustomerAsync(id, viewCustomer))
     .ThrowsAsync(new KeyNotFoundException("Customer not found"));

        // Act
        var result = await _controller.UpdateCustomer(id, viewCustomer);

     // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Contains("Customer not found", notFoundResult.Value?.ToString());
    }

    /// <summary>
    /// Tests that UpdateCustomer returns Conflict when email exists for another customer.
    /// </summary>
    [Fact]
    public async Task UpdateCustomer_ReturnsConflict_WhenEmailExistsForAnotherCustomer()
    {
   // Arrange
  var id = 1;
 var viewCustomer = new POSViewCustomer
        {
            Name = "John Doe",
       Email = "jane@test.com",
            BillingAddress = "123 Main St"
        };

_mockService.Setup(s => s.UpdateCustomerAsync(id, viewCustomer))
       .ThrowsAsync(new InvalidOperationException("Email already exists"));

 // Act
   var result = await _controller.UpdateCustomer(id, viewCustomer);

        // Assert
     Assert.IsType<ConflictObjectResult>(result.Result);
    }

    #endregion

    #region DeleteCustomer Tests

    /// <summary>
    /// Tests that DeleteCustomer returns NoContent when successful.
    /// </summary>
    [Fact]
    public async Task DeleteCustomer_ReturnsNoContent_WhenSuccessful()
    {
        // Arrange
        var id = 1;
        _mockService.Setup(s => s.DeleteCustomerAsync(id)).ReturnsAsync(true);

  // Act
    var result = await _controller.DeleteCustomer(id);

        // Assert
   Assert.IsType<NoContentResult>(result);
    }

    /// <summary>
    /// Tests that DeleteCustomer returns NotFound when customer doesn't exist.
    /// </summary>
    [Fact]
    public async Task DeleteCustomer_ReturnsNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        var id = 999;
  _mockService.Setup(s => s.DeleteCustomerAsync(id)).ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteCustomer(id);

   // Assert
   var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains($"Customer with ID {id} not found", notFoundResult.Value?.ToString());
    }

    /// <summary>
    /// Tests that DeleteCustomer returns BadRequest for invalid ID.
    /// </summary>
    [Fact]
    public async Task DeleteCustomer_ReturnsBadRequest_ForInvalidId()
  {
    // Arrange
 var invalidId = 0;
        _mockService.Setup(s => s.DeleteCustomerAsync(invalidId))
       .ThrowsAsync(new ArgumentException("Invalid ID"));

   // Act
     var result = await _controller.DeleteCustomer(invalidId);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    #endregion
}
