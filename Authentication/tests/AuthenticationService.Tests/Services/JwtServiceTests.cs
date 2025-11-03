using AuthenticationService.Configuration;
using AuthenticationService.Models;
using AuthenticationService.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthenticationService.Tests.Services;

/// <summary>
/// Unit tests for JwtService following xUnit best practices
/// Tests JWT token generation, validation, and expiry
/// </summary>
public class JwtServiceTests
{
    private readonly Mock<ILogger<JwtService>> _mockLogger;
    private readonly JwtSettings _jwtSettings;
private readonly JwtService _jwtService;

    public JwtServiceTests()
    {
  // Arrange - Constructor setup for all tests
        _mockLogger = new Mock<ILogger<JwtService>>();
        _jwtSettings = new JwtSettings
        {
        Key = "ThisIsAVerySecureKeyForTestingPurposesOnly123456",
            Issuer = "TestIssuer",
      Audience = "TestAudience",
            ExpiryInMinutes = 60
        };
        _jwtService = new JwtService(_jwtSettings, _mockLogger.Object);
    }

    #region GenerateToken Tests

    [Fact]
    [Trait("Category", "JwtService")]
    public void GenerateToken_ValidUser_ReturnsValidJwtToken()
    {
        // Arrange
        var user = new User
        {
 Id = 1,
            Name = "Test User",
            Email = "test@example.com"
     };

        // Act
     var token = _jwtService.GenerateToken(user);

        // Assert
token.Should().NotBeNullOrEmpty();
      var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(token).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "JwtService")]
    public void GenerateToken_ValidUser_ContainsCorrectClaims()
    {
     // Arrange
        var user = new User
   {
      Id = 42,
  Name = "John Doe",
       Email = "john.doe@example.com"
        };

        // Act
        var token = _jwtService.GenerateToken(user);

 // Assert
  var handler = new JwtSecurityTokenHandler();
  var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "42");
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "john.doe@example.com");
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Name && c.Value == "John Doe");
      jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti);
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Iat);
    }

    [Fact]
    [Trait("Category", "JwtService")]
    public void GenerateToken_ValidUser_HasCorrectIssuerAndAudience()
    {
        // Arrange
      var user = new User
        {
        Id = 1,
     Name = "Test User",
      Email = "test@example.com"
        };

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Issuer.Should().Be("TestIssuer");
    jwtToken.Audiences.Should().Contain("TestAudience");
    }

    [Fact]
    [Trait("Category", "JwtService")]
    public void GenerateToken_ValidUser_HasCorrectExpiryTime()
    {
        // Arrange
   var user = new User
        {
  Id = 1,
      Name = "Test User",
     Email = "test@example.com"
        };
 var beforeGeneration = DateTime.UtcNow;

        // Act
   var token = _jwtService.GenerateToken(user);
     var afterGeneration = DateTime.UtcNow;

        // Assert
        var handler = new JwtSecurityTokenHandler();
   var jwtToken = handler.ReadJwtToken(token);

        var expectedExpiry = beforeGeneration.AddMinutes(_jwtSettings.ExpiryInMinutes);
        jwtToken.ValidTo.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(1, "user1@test.com", "User One")]
    [InlineData(999, "admin@test.com", "Admin User")]
[InlineData(12345, "test.email+tag@example.org", "Complex Name With Spaces")]
    [Trait("Category", "JwtService")]
    public void GenerateToken_DifferentUsers_GeneratesDifferentTokens(int userId, string email, string name)
    {
        // Arrange
   var user = new User
      {
            Id = userId,
 Name = name,
 Email = email
        };

        // Act
     var token = _jwtService.GenerateToken(user);

   // Assert
        var handler = new JwtSecurityTokenHandler();
     var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == userId.ToString());
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == email);
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Name && c.Value == name);
    }

    [Fact]
    [Trait("Category", "JwtService")]
    public void GenerateToken_CalledMultipleTimes_GeneratesUniqueJti()
  {
        // Arrange
        var user = new User
        {
       Id = 1,
            Name = "Test User",
            Email = "test@example.com"
        };

        // Act
        var token1 = _jwtService.GenerateToken(user);
 var token2 = _jwtService.GenerateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
   var jwtToken1 = handler.ReadJwtToken(token1);
var jwtToken2 = handler.ReadJwtToken(token2);

        var jti1 = jwtToken1.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        var jti2 = jwtToken2.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

   jti1.Should().NotBe(jti2);
    }

    [Fact]
    [Trait("Category", "JwtService")]
  public void GenerateToken_ValidUser_LogsInformation()
    {
      // Arrange
        var user = new User
        {
            Id = 1,
        Name = "Test User",
Email = "test@example.com"
        };

        // Act
   _jwtService.GenerateToken(user);

   // Assert
        _mockLogger.Verify(
    x => x.Log(
    LogLevel.Information,
  It.IsAny<EventId>(),
   It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("JWT token generated")),
           It.IsAny<Exception>(),
   It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
       Times.Once);
    }

    [Fact]
    [Trait("Category", "JwtService")]
    public void GenerateToken_UserWithSpecialCharactersInEmail_GeneratesValidToken()
    {
        // Arrange
var user = new User
    {
   Id = 1,
            Name = "Test User",
            Email = "test+tag@sub.example.com"
        };

        // Act
  var token = _jwtService.GenerateToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
   var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "test+tag@sub.example.com");
    }

    [Fact]
    [Trait("Category", "JwtService")]
    public void GenerateToken_UserWithUnicodeCharactersInName_GeneratesValidToken()
    {
        // Arrange
        var user = new User
        {
   Id = 1,
            Name = "José García ??",
  Email = "test@example.com"
        };

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
 token.Should().NotBeNullOrEmpty();
        var handler = new JwtSecurityTokenHandler();
var jwtToken = handler.ReadJwtToken(token);
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Name && c.Value == "José García ??");
 }

    #endregion

    #region GetTokenExpiry Tests

    [Fact]
    [Trait("Category", "JwtService")]
    public void GetTokenExpiry_ReturnsCorrectExpiryTime()
    {
        // Arrange
var beforeCall = DateTime.UtcNow;

     // Act
    var expiry = _jwtService.GetTokenExpiry();
        var afterCall = DateTime.UtcNow;

        // Assert
        var expectedExpiry = beforeCall.AddMinutes(_jwtSettings.ExpiryInMinutes);
        expiry.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(2));
 expiry.Should().BeAfter(afterCall);
    }

    [Fact]
    [Trait("Category", "JwtService")]
    public void GetTokenExpiry_CalledMultipleTimes_ReturnsUpdatedTime()
 {
        // Act
        var expiry1 = _jwtService.GetTokenExpiry();
        Thread.Sleep(1000); // Wait 1 second
        var expiry2 = _jwtService.GetTokenExpiry();

        // Assert
        expiry2.Should().BeAfter(expiry1);
    }

    [Fact]
    [Trait("Category", "JwtService")]
    public void GetTokenExpiry_WithCustomExpiryMinutes_ReturnsCorrectTime()
    {
    // Arrange
        var customSettings = new JwtSettings
     {
            Key = "ThisIsAVerySecureKeyForTestingPurposesOnly123456",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
     ExpiryInMinutes = 120 // 2 hours
        };
        var customJwtService = new JwtService(customSettings, _mockLogger.Object);
      var beforeCall = DateTime.UtcNow;

// Act
        var expiry = customJwtService.GetTokenExpiry();

        // Assert
        var expectedExpiry = beforeCall.AddMinutes(120);
        expiry.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(2));
    }

    #endregion

    #region Edge Cases and Negative Tests

    [Fact]
    [Trait("Category", "JwtService")]
    public void GenerateToken_UserWithZeroId_GeneratesValidToken()
    {
// Arrange
        var user = new User
        {
       Id = 0,
            Name = "Test User",
            Email = "test@example.com"
        };

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
   token.Should().NotBeNullOrEmpty();
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "0");
    }

    [Fact]
    [Trait("Category", "JwtService")]
    public void GenerateToken_UserWithEmptyName_GeneratesValidToken()
    {
  // Arrange
        var user = new User
        {
            Id = 1,
        Name = string.Empty,
   Email = "test@example.com"
        };

        // Act
        var token = _jwtService.GenerateToken(user);

  // Assert
        token.Should().NotBeNullOrEmpty();
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
    jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Name && c.Value == string.Empty);
  }

    [Fact]
    [Trait("Category", "JwtService")]
    public void GenerateToken_UserWithEmptyEmail_GeneratesValidToken()
    {
    // Arrange
        var user = new User
        {
            Id = 1,
     Name = "Test User",
            Email = string.Empty
        };

        // Act
        var token = _jwtService.GenerateToken(user);

    // Assert
        token.Should().NotBeNullOrEmpty();
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
      jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == string.Empty);
    }

    [Fact]
    [Trait("Category", "JwtService")]
    public void GenerateToken_UserWithVeryLongEmail_GeneratesValidToken()
    {
      // Arrange
      var longEmail = new string('a', 200) + "@example.com";
      var user = new User
        {
            Id = 1,
  Name = "Test User",
            Email = longEmail
        };

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        var handler = new JwtSecurityTokenHandler();
      var jwtToken = handler.ReadJwtToken(token);
        jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == longEmail);
    }

    [Fact]
    [Trait("Category", "JwtService")]
    public void GenerateToken_UserWithNegativeId_GeneratesValidToken()
  {
        // Arrange
        var user = new User
     {
    Id = -1,
            Name = "Test User",
    Email = "test@example.com"
    };

     // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
    token.Should().NotBeNullOrEmpty();
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
     jwtToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "-1");
    }

    #endregion
}
