using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using First_Sample_Project_Prompting.Models;
using First_Sample_Project_Prompting.Repositories;
using First_Sample_Project_Prompting.Services;

namespace CustomerApi.Tests.Services;

/// <summary>
/// Unit tests for CustomerService class using XUnit framework.
/// </summary>
public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _mockRepository;
    private readonly Mock<ILogger<CustomerService>> _mockLogger;
    private readonly CustomerService _customerService;

  /// <summary>
    /// Initializes a new instance of the test class.
    /// Constructor is called before each test method (XUnit behavior).
    /// </summary>
    public CustomerServiceTests()
    {
        _mockRepository = new Mock<ICustomerRepository>();
        _mockLogger = new Mock<ILogger<CustomerService>>();
        _customerService = new CustomerService(_mockRepository.Object, _mockLogger.Object);
    }

  #region GetAllCustomersAsync Tests

    /// <summary>
    /// Tests that GetAllCustomersAsync returns all customers successfully.
  /// </summary>
    [Fact]
    public async Task GetAllCustomersAsync_ReturnsAllCustomers_Successfully()
    {
        // Arrange
        var expectedCustomers = new List<POSCustomer>
    {
        new() { Id = 1, Name = "John Doe", Email = "john@test.com", billing_address = "123 Main St" },
            new() { Id = 2, Name = "Jane Smith", Email = "jane@test.com", billing_address = "456 Oak Ave" }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(expectedCustomers);

        // Act
      var result = await _customerService.GetAllCustomersAsync();

        // Assert
     Assert.NotNull(result);
      Assert.Equal(2, result.Count());
   _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that GetAllCustomersAsync returns empty list when no customers exist.
    /// </summary>
    [Fact]
    public async Task GetAllCustomersAsync_ReturnsEmptyList_WhenNoCustomersExist()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<POSCustomer>());

        // Act
        var result = await _customerService.GetAllCustomersAsync();

        // Assert
        Assert.NotNull(result);
  Assert.Empty(result);
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    #endregion

    #region GetCustomerByIdAsync Tests

/// <summary>
  /// Tests that GetCustomerByIdAsync returns customer when found.
    /// </summary>
    [Fact]
    public async Task GetCustomerByIdAsync_ReturnsCustomer_WhenCustomerExists()
    {
        // Arrange
  var id = 1;
    var expectedCustomer = new POSCustomer 
        { 
Id = id, 
 Name = "John Doe", 
        Email = "john@test.com",
   billing_address = "123 Main St"
        };
        _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(expectedCustomer);

        // Act
        var result = await _customerService.GetCustomerByIdAsync(id);

        // Assert
        Assert.NotNull(result);
  Assert.Equal(id, result!.Id);
        Assert.Equal("John Doe", result.Name);
        _mockRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    /// <summary>
  /// Tests that GetCustomerByIdAsync returns null when customer not found.
    /// </summary>
    [Fact]
    public async Task GetCustomerByIdAsync_ReturnsNull_WhenCustomerNotFound()
    {
        // Arrange
    var id = 999;
   _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((POSCustomer?)null);

  // Act
        var result = await _customerService.GetCustomerByIdAsync(id);

   // Assert
    Assert.Null(result);
        _mockRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
    }

    /// <summary>
    /// Tests that GetCustomerByIdAsync throws exception for invalid ID.
    /// </summary>
  [Fact]
    public async Task GetCustomerByIdAsync_ThrowsArgumentException_WhenIdIsInvalid()
    {
        // Arrange
        var invalidId = 0;

 // Act & Assert
  var exception = await Assert.ThrowsAsync<ArgumentException>(
     async () => await _customerService.GetCustomerByIdAsync(invalidId));
    
        Assert.Contains("Customer ID must be greater than zero", exception.Message);
    }

    /// <summary>
    /// Tests that GetCustomerByIdAsync throws exception for negative ID.
    /// </summary>
    [Fact]
    public async Task GetCustomerByIdAsync_ThrowsArgumentException_WhenIdIsNegative()
    {
   // Arrange
        var negativeId = -1;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _customerService.GetCustomerByIdAsync(negativeId));
    }

    #endregion

    #region CreateCustomerAsync Tests

    /// <summary>
    /// Tests that CreateCustomerAsync creates customer successfully.
    /// </summary>
    [Fact]
    public async Task CreateCustomerAsync_CreatesCustomer_Successfully()
    {
        // Arrange
      var viewCustomer = new POSViewCustomer
        {
      Name = "John Doe",
        Email = "john@test.com",
Phone = "555-0100",
            billing_address = "123 Main St"
  };

        _mockRepository.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), null)).ReturnsAsync(false);
      _mockRepository.Setup(r => r.CreateAsync(It.IsAny<POSCustomer>()))
            .ReturnsAsync((POSCustomer c) => { c.Id = 1; return c; });

        // Act
        var result = await _customerService.CreateCustomerAsync(viewCustomer);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("john@test.com", result.Email);
        _mockRepository.Verify(r => r.CreateAsync(It.IsAny<POSCustomer>()), Times.Once);
    }

    /// <summary>
    /// Tests that CreateCustomerAsync throws exception when email already exists.
    /// </summary>
    [Fact]
    public async Task CreateCustomerAsync_ThrowsInvalidOperationException_WhenEmailExists()
    {
        // Arrange
        var viewCustomer = new POSViewCustomer
        {
     Name = "John Doe",
     Email = "john@test.com",
       billing_address = "123 Main St"
        };
      
        _mockRepository.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), null)).ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
     async () => await _customerService.CreateCustomerAsync(viewCustomer));
        
        Assert.Contains("already exists", exception.Message);
    }

    /// <summary>
    /// Tests that CreateCustomerAsync throws exception when input is null.
    /// </summary>
    [Fact]
    public async Task CreateCustomerAsync_ThrowsArgumentNullException_WhenInputIsNull()
    {
    // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _customerService.CreateCustomerAsync(null!));
        
        Assert.Equal("viewCustomer", exception.ParamName);
    }

    /// <summary>
    /// Tests that CreateCustomerAsync throws exception for invalid email format.
    /// </summary>
    [Fact]
    public async Task CreateCustomerAsync_ThrowsArgumentException_WhenEmailIsInvalid()
    {
// Arrange
        var viewCustomer = new POSViewCustomer
        {
       Name = "John Doe",
Email = "invalid-email",
    billing_address = "123 Main St"
        };

    // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _customerService.CreateCustomerAsync(viewCustomer));
    }

    #endregion

    #region UpdateCustomerAsync Tests

    /// <summary>
    /// Tests that UpdateCustomerAsync updates customer successfully.
    /// </summary>
    [Fact]
    public async Task UpdateCustomerAsync_UpdatesCustomer_Successfully()
    {
        // Arrange
    var id = 1;
        var existingCustomer = new POSCustomer
        {
         Id = id,
  Name = "John Doe",
      Email = "john@test.com",
            billing_address = "123 Main St"
        };
   
        var viewCustomer = new POSViewCustomer
        {
  Name = "John Updated",
 Email = "john@test.com",
  billing_address = "123 Main St"
      };

        _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingCustomer);
        _mockRepository.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), id)).ReturnsAsync(false);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<POSCustomer>()))
            .ReturnsAsync((POSCustomer c) => c);

        // Act
   var result = await _customerService.UpdateCustomerAsync(id, viewCustomer);

 // Assert
        Assert.NotNull(result);
        Assert.Equal("John Updated", result.Name);
     _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<POSCustomer>()), Times.Once);
    }

    /// <summary>
    /// Tests that UpdateCustomerAsync throws exception when customer not found.
    /// </summary>
    [Fact]
 public async Task UpdateCustomerAsync_ThrowsKeyNotFoundException_WhenCustomerNotFound()
    {
        // Arrange
        var id = 999;
        var viewCustomer = new POSViewCustomer
        {
  Name = "John Doe",
            Email = "john@test.com",
            billing_address = "123 Main St"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((POSCustomer?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _customerService.UpdateCustomerAsync(id, viewCustomer));
        
  Assert.Contains("not found", exception.Message);
    }

    /// <summary>
    /// Tests that UpdateCustomerAsync throws exception when email already exists for another customer.
    /// </summary>
    [Fact]
    public async Task UpdateCustomerAsync_ThrowsInvalidOperationException_WhenEmailExistsForAnotherCustomer()
    {
      // Arrange
        var id = 1;
      var existingCustomer = new POSCustomer
 {
            Id = id,
            Name = "John Doe",
      Email = "john@test.com",
   billing_address = "123 Main St"
      };
        
   var viewCustomer = new POSViewCustomer
    {
            Name = "John Doe",
     Email = "jane@test.com", // Different email that belongs to another customer
            billing_address = "123 Main St"
};

  _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingCustomer);
     _mockRepository.Setup(r => r.EmailExistsAsync("jane@test.com", id)).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _customerService.UpdateCustomerAsync(id, viewCustomer));
    }

  #endregion

    #region DeleteCustomerAsync Tests

    /// <summary>
    /// Tests that DeleteCustomerAsync deletes customer successfully.
  /// </summary>
    [Fact]
    public async Task DeleteCustomerAsync_DeletesCustomer_Successfully()
 {
        // Arrange
      var id = 1;
        var existingCustomer = new POSCustomer
     {
            Id = id,
          Name = "John Doe",
    Email = "john@test.com",
            billing_address = "123 Main St"
   };

   _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existingCustomer);
        _mockRepository.Setup(r => r.DeleteAsync(id)).ReturnsAsync(true);

        // Act
     var result = await _customerService.DeleteCustomerAsync(id);

        // Assert
   Assert.True(result);
    _mockRepository.Verify(r => r.DeleteAsync(id), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteCustomerAsync returns false when customer not found.
    /// </summary>
    [Fact]
    public async Task DeleteCustomerAsync_ReturnsFalse_WhenCustomerNotFound()
    {
        // Arrange
        var id = 999;
        _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((POSCustomer?)null);

  // Act
  var result = await _customerService.DeleteCustomerAsync(id);

        // Assert
        Assert.False(result);
  _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    /// <summary>
    /// Tests that DeleteCustomerAsync throws exception for invalid ID.
    /// </summary>
    [Fact]
    public async Task DeleteCustomerAsync_ThrowsArgumentException_WhenIdIsInvalid()
    {
        // Arrange
  var invalidId = 0;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _customerService.DeleteCustomerAsync(invalidId));
    }

    #endregion
}
