using AuthenticationService.Controllers;
using AuthenticationService.DTOs;
using AuthenticationService.Models;
using AuthenticationService.Repositories;
using AuthenticationService.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace AuthenticationService.Tests.Controllers;

/// <summary>
/// Unit tests for AuthController following xUnit best practices
/// Tests user registration and login functionality
/// </summary>
public class AuthControllerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IJwtService> _mockJwtService;
  private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        // Arrange - Constructor setup for all tests
        _mockUserRepository = new Mock<IUserRepository>();
        _mockJwtService = new Mock<IJwtService>();
        _mockLogger = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(
     _mockUserRepository.Object,
            _mockJwtService.Object,
            _mockLogger.Object
        );
    }

    #region Register Tests - Success Cases

    [Fact]
    [Trait("Category", "AuthController")]
    public async Task Register_ValidRequest_ReturnsCreatedResult()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Name = "John Doe",
     Email = "john@example.com",
     Password = "SecurePassword123"
        };

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        _mockUserRepository
     .Setup(x => x.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(new User
     {
    Id = 1,
    Name = request.Name,
     Email = request.Email,
                PasswordHash = "hashedPassword"
            });

        // Act
        var result = await _controller.Register(request);

        // Assert
      result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
  createdResult!.StatusCode.Should().Be(201);
    
    var response = createdResult.Value as RegisterResponse;
   response.Should().NotBeNull();
        response!.UserId.Should().Be(1);
        response.Message.Should().Be("User registered successfully");
    }

    [Fact]
    [Trait("Category", "AuthController")]
    public async Task Register_ValidRequest_HashesPassword()
    {
   // Arrange
var request = new RegisterRequest
        {
            Name = "Jane Doe",
     Email = "jane@example.com",
            Password = "MySecretPass123"
        };

        User? capturedUser = null;
        _mockUserRepository
     .Setup(x => x.GetByEmailAsync(request.Email))
     .ReturnsAsync((User?)null);

        _mockUserRepository
      .Setup(x => x.CreateAsync(It.IsAny<User>()))
  .Callback<User>(u => capturedUser = u)
.ReturnsAsync((User u) => { u.Id = 1; return u; });

        // Act
    await _controller.Register(request);

        // Assert
   capturedUser.Should().NotBeNull();
        capturedUser!.PasswordHash.Should().NotBeNullOrEmpty();
        capturedUser.PasswordHash.Should().NotBe(request.Password); // Password should be hashed, not plain text
        capturedUser.PasswordHash.Should().StartWith("$2"); // BCrypt hash starts with $2
    }

    [Fact]
    [Trait("Category", "AuthController")]
    public async Task Register_ValidRequest_CreatesUserWithCorrectData()
    {
        // Arrange
        var request = new RegisterRequest
        {
          Name = "Test User",
    Email = "test@example.com",
        Password = "Password123"
        };

        User? capturedUser = null;
        _mockUserRepository
       .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        _mockUserRepository
            .Setup(x => x.CreateAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
     .ReturnsAsync((User u) => { u.Id = 5; return u; });

        // Act
        await _controller.Register(request);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.Name.Should().Be(request.Name);
        capturedUser.Email.Should().Be(request.Email);
    }

    [Theory]
    [InlineData("user@example.com", "User One", "Pass1234")]
    [InlineData("admin@test.org", "Admin User", "SecureP@ss")]
    [InlineData("test+tag@email.com", "Complex User", "MyPassword!123")]
    [Trait("Category", "AuthController")]
    public async Task Register_DifferentValidRequests_ReturnsCreatedResult(string email, string name, string password)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = email,
 Name = name,
            Password = password
        };

   _mockUserRepository
  .Setup(x => x.GetByEmailAsync(email))
  .ReturnsAsync((User?)null);

        _mockUserRepository
      .Setup(x => x.CreateAsync(It.IsAny<User>()))
        .ReturnsAsync(new User { Id = 1, Name = name, Email = email });

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    #endregion

    #region Register Tests - Failure Cases

    [Fact]
    [Trait("Category", "AuthController")]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        // Arrange
        var request = new RegisterRequest
        {
  Name = "John Doe",
      Email = "existing@example.com",
     Password = "Password123"
     };

        var existingUser = new User
        {
  Id = 1,
 Email = "existing@example.com",
            Name = "Existing User"
     };

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(request.Email))
      .ReturnsAsync(existingUser);

        // Act
    var result = await _controller.Register(request);

   // Assert
        result.Should().BeOfType<ConflictObjectResult>();
        var conflictResult = result as ConflictObjectResult;
        conflictResult!.StatusCode.Should().Be(409);

        var errorResponse = conflictResult.Value as ErrorResponse;
  errorResponse.Should().NotBeNull();
      errorResponse!.Error.Should().Be("Email already exists");
    }

    [Fact]
    [Trait("Category", "AuthController")]
    public async Task Register_DuplicateEmail_DoesNotCreateUser()
    {
        // Arrange
      var request = new RegisterRequest
        {
     Name = "John Doe",
            Email = "existing@example.com",
    Password = "Password123"
        };

 _mockUserRepository
 .Setup(x => x.GetByEmailAsync(request.Email))
   .ReturnsAsync(new User { Id = 1, Email = request.Email });

   // Act
        await _controller.Register(request);

        // Assert
        _mockUserRepository.Verify(
       x => x.CreateAsync(It.IsAny<User>()),
          Times.Never);
    }

  [Fact]
    [Trait("Category", "AuthController")]
    public async Task Register_RepositoryThrowsException_ReturnsInternalServerError()
    {
   // Arrange
        var request = new RegisterRequest
        {
        Name = "John Doe",
            Email = "john@example.com",
            Password = "Password123"
  };

        _mockUserRepository
       .Setup(x => x.GetByEmailAsync(request.Email))
        .ReturnsAsync((User?)null);

        _mockUserRepository
            .Setup(x => x.CreateAsync(It.IsAny<User>()))
    .ThrowsAsync(new Exception("Database error"));

        // Act
     var result = await _controller.Register(request);

        // Assert
    result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
     objectResult!.StatusCode.Should().Be(500);

 var errorResponse = objectResult.Value as ErrorResponse;
      errorResponse.Should().NotBeNull();
        errorResponse!.Error.Should().Be("An error occurred during registration");
    }

    #endregion

    #region Login Tests - Success Cases

  [Fact]
    [Trait("Category", "AuthController")]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var password = "Password123";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

        var request = new LoginRequest
        {
            Email = "john@example.com",
      Password = password
        };

        var user = new User
        {
        Id = 1,
        Email = request.Email,
            Name = "John Doe",
    PasswordHash = passwordHash
        };

        var expectedToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.test.token";
        var expectedExpiry = DateTime.UtcNow.AddHours(1);

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);

  _mockJwtService
            .Setup(x => x.GenerateToken(user))
  .Returns(expectedToken);

        _mockJwtService
  .Setup(x => x.GetTokenExpiry())
            .Returns(expectedExpiry);

        // Act
   var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
    okResult!.StatusCode.Should().Be(200);

        var response = okResult.Value as LoginResponse;
        response.Should().NotBeNull();
        response!.Token.Should().Be(expectedToken);
        response.ExpiresAt.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(1));
}

    [Fact]
    [Trait("Category", "AuthController")]
    public async Task Login_ValidCredentials_CallsJwtServiceWithCorrectUser()
{
        // Arrange
   var password = "SecurePassword";
   var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var request = new LoginRequest
    {
            Email = "test@example.com",
Password = password
        };

      var user = new User
        {
       Id = 42,
            Email = request.Email,
      Name = "Test User",
         PasswordHash = passwordHash
   };

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(request.Email))
   .ReturnsAsync(user);

  _mockJwtService
    .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("token");

        _mockJwtService
   .Setup(x => x.GetTokenExpiry())
            .Returns(DateTime.UtcNow.AddHours(1));

   // Act
 await _controller.Login(request);

        // Assert
     _mockJwtService.Verify(
       x => x.GenerateToken(It.Is<User>(u => 
          u.Id == 42 && 
                u.Email == "test@example.com" && 
     u.Name == "Test User")),
            Times.Once);
    }

    [Theory]
    [InlineData("user@example.com", "Password123")]
    [InlineData("admin@test.org", "AdminP@ss456")]
  [InlineData("test+tag@email.com", "MySecurePass!")]
    [Trait("Category", "AuthController")]
    public async Task Login_DifferentValidCredentials_ReturnsOkWithToken(string email, string password)
    {
        // Arrange
 var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var request = new LoginRequest
        {
     Email = email,
            Password = password
        };

        var user = new User
        {
        Id = 1,
      Email = email,
    Name = "Test User",
  PasswordHash = passwordHash
        };

        _mockUserRepository
        .Setup(x => x.GetByEmailAsync(email))
     .ReturnsAsync(user);

   _mockJwtService
            .Setup(x => x.GenerateToken(user))
            .Returns("valid.jwt.token");

        _mockJwtService
         .Setup(x => x.GetTokenExpiry())
   .Returns(DateTime.UtcNow.AddHours(1));

     // Act
        var result = await _controller.Login(request);

     // Assert
      result.Should().BeOfType<OkObjectResult>();
    }

  #endregion

    #region Login Tests - Failure Cases

    [Fact]
    [Trait("Category", "AuthController")]
    public async Task Login_NonExistentEmail_ReturnsUnauthorized()
    {
        // Arrange
  var request = new LoginRequest
    {
            Email = "nonexistent@example.com",
            Password = "Password123"
    };

     _mockUserRepository
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.Login(request);

     // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result as UnauthorizedObjectResult;
        unauthorizedResult!.StatusCode.Should().Be(401);

      var errorResponse = unauthorizedResult.Value as ErrorResponse;
  errorResponse.Should().NotBeNull();
        errorResponse!.Error.Should().Be("Invalid email or password");
    }

    [Fact]
    [Trait("Category", "AuthController")]
    public async Task Login_IncorrectPassword_ReturnsUnauthorized()
    {
        // Arrange
        var correctPassword = "CorrectPassword123";
        var incorrectPassword = "WrongPassword456";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(correctPassword);

var request = new LoginRequest
        {
            Email = "john@example.com",
  Password = incorrectPassword
      };

   var user = new User
 {
    Id = 1,
   Email = request.Email,
    Name = "John Doe",
            PasswordHash = passwordHash
        };

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(request.Email))
 .ReturnsAsync(user);

        // Act
        var result = await _controller.Login(request);

        // Assert
      result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result as UnauthorizedObjectResult;

        var errorResponse = unauthorizedResult!.Value as ErrorResponse;
        errorResponse!.Error.Should().Be("Invalid email or password");
    }

    [Fact]
    [Trait("Category", "AuthController")]
public async Task Login_IncorrectPassword_DoesNotGenerateToken()
    {
        // Arrange
      var correctPassword = "CorrectPassword123";
      var incorrectPassword = "WrongPassword456";
    var passwordHash = BCrypt.Net.BCrypt.HashPassword(correctPassword);

        var request = new LoginRequest
        {
            Email = "john@example.com",
  Password = incorrectPassword
   };

        var user = new User
        {
            Id = 1,
            Email = request.Email,
            PasswordHash = passwordHash
        };

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(request.Email))
   .ReturnsAsync(user);

        // Act
        await _controller.Login(request);

    // Assert
        _mockJwtService.Verify(
            x => x.GenerateToken(It.IsAny<User>()),
    Times.Never);
    }

    [Fact]
    [Trait("Category", "AuthController")]
    public async Task Login_RepositoryThrowsException_ReturnsInternalServerError()
    {
      // Arrange
        var request = new LoginRequest
   {
       Email = "john@example.com",
        Password = "Password123"
        };

   _mockUserRepository
  .Setup(x => x.GetByEmailAsync(request.Email))
      .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.Login(request);

        // Assert
  result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
  objectResult!.StatusCode.Should().Be(500);

        var errorResponse = objectResult.Value as ErrorResponse;
   errorResponse!.Error.Should().Be("An error occurred during login");
 }

    [Fact]
    [Trait("Category", "AuthController")]
    public async Task Login_JwtServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
  var password = "Password123";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

 var request = new LoginRequest
        {
            Email = "john@example.com",
            Password = password
        };

        var user = new User
        {
       Id = 1,
            Email = request.Email,
     PasswordHash = passwordHash
        };

        _mockUserRepository
  .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);

  _mockJwtService
      .Setup(x => x.GenerateToken(user))
    .Throws(new Exception("JWT generation error"));

 // Act
      var result = await _controller.Login(request);

      // Assert
        result.Should().BeOfType<ObjectResult>();
   var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
  }

    #endregion

    #region Edge Cases and Security Tests

    [Fact]
    [Trait("Category", "AuthController")]
    public async Task Login_CaseSensitivePassword_FailsWithWrongCase()
    {
        // Arrange
  var correctPassword = "Password123";
   var incorrectPassword = "password123"; // Different case
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(correctPassword);

        var request = new LoginRequest
    {
 Email = "john@example.com",
            Password = incorrectPassword
        };

        var user = new User
        {
  Id = 1,
   Email = request.Email,
       PasswordHash = passwordHash
        };

        _mockUserRepository
       .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync(user);

    // Act
        var result = await _controller.Login(request);

     // Assert
  result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    [Trait("Category", "AuthController")]
    public async Task Register_BCryptWorkFactor_Uses12Rounds()
    {
        // Arrange
      var request = new RegisterRequest
        {
            Name = "Security Test",
     Email = "security@example.com",
      Password = "TestPassword123"
    };

   User? capturedUser = null;
        _mockUserRepository
         .Setup(x => x.GetByEmailAsync(request.Email))
      .ReturnsAsync((User?)null);

        _mockUserRepository
  .Setup(x => x.CreateAsync(It.IsAny<User>()))
          .Callback<User>(u => capturedUser = u)
            .ReturnsAsync((User u) => { u.Id = 1; return u; });

        // Act
        await _controller.Register(request);

        // Assert
    capturedUser.Should().NotBeNull();
        // BCrypt hash with work factor 12 should start with $2a$12$ or $2b$12$
        capturedUser!.PasswordHash.Should().MatchRegex(@"^\$2[ab]\$12\$");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [Trait("Category", "AuthController")]
    public async Task Register_EmptyOrWhitespacePassword_ShouldStillHash(string password)
    {
        // Arrange
        var request = new RegisterRequest
        {
        Name = "Test User",
       Email = "test@example.com",
            Password = password
      };

        User? capturedUser = null;
        _mockUserRepository
            .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

     _mockUserRepository
 .Setup(x => x.CreateAsync(It.IsAny<User>()))
         .Callback<User>(u => capturedUser = u)
     .ReturnsAsync((User u) => { u.Id = 1; return u; });

        // Act
        await _controller.Register(request);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.PasswordHash.Should().NotBeNullOrEmpty();
        capturedUser.PasswordHash.Should().NotBe(password);
    }

    [Fact]
    [Trait("Category", "AuthController")]
    public async Task Register_VeryLongPassword_HashesSuccessfully()
    {
  // Arrange
        var longPassword = new string('a', 1000);
    var request = new RegisterRequest
        {
    Name = "Test User",
        Email = "test@example.com",
  Password = longPassword
        };

        User? capturedUser = null;
        _mockUserRepository
         .Setup(x => x.GetByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        _mockUserRepository
    .Setup(x => x.CreateAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
     .ReturnsAsync((User u) => { u.Id = 1; return u; });

        // Act
        await _controller.Register(request);

// Assert
   capturedUser.Should().NotBeNull();
     capturedUser!.PasswordHash.Should().NotBeNullOrEmpty();
        capturedUser.PasswordHash.Should().StartWith("$2");
    }

    [Fact]
    [Trait("Category", "AuthController")]
    public async Task Register_SpecialCharactersInPassword_HashesCorrectly()
    {
        // Arrange
        var specialPassword = "P@ssw0rd!#$%^&*()";
        var request = new RegisterRequest
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = specialPassword
        };

     User? capturedUser = null;
        _mockUserRepository
         .Setup(x => x.GetByEmailAsync(request.Email))
 .ReturnsAsync((User?)null);

        _mockUserRepository
.Setup(x => x.CreateAsync(It.IsAny<User>()))
            .Callback<User>(u => capturedUser = u)
            .ReturnsAsync((User u) => { u.Id = 1; return u; });

        // Act
      await _controller.Register(request);

        // Assert
        capturedUser.Should().NotBeNull();
        capturedUser!.PasswordHash.Should().NotBeNullOrEmpty();
     
      // Verify the hash can verify the original password
        var isValid = BCrypt.Net.BCrypt.Verify(specialPassword, capturedUser.PasswordHash);
        isValid.Should().BeTrue();
    }

    #endregion
}
