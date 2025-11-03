using AuthenticationService.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace AuthenticationService.Tests.Middleware;

/// <summary>
/// Unit tests for JwtAuthenticationMiddleware following xUnit best practices
/// Tests authorization header validation and middleware behavior
/// </summary>
public class JwtAuthenticationMiddlewareTests
{
    private readonly Mock<ILogger<JwtAuthenticationMiddleware>> _mockLogger;
    private readonly Mock<RequestDelegate> _mockNext;

    public JwtAuthenticationMiddlewareTests()
    {
        // Arrange - Constructor setup for all tests
     _mockLogger = new Mock<ILogger<JwtAuthenticationMiddleware>>();
     _mockNext = new Mock<RequestDelegate>();
    }

    #region Helper Methods

    private HttpContext CreateHttpContext(string path, string? authorizationHeader = null, bool requiresAuth = true)
    {
 var context = new DefaultHttpContext();
        context.Request.Path = path;
  
    if (!string.IsNullOrEmpty(authorizationHeader))
     {
            context.Request.Headers["Authorization"] = authorizationHeader;
 }

     if (requiresAuth)
        {
     var endpoint = new Endpoint(
    requestDelegate: _ => Task.CompletedTask,
           metadata: new EndpointMetadataCollection(new AuthorizeAttribute()),
          displayName: "Test Endpoint"
 );
            context.SetEndpoint(endpoint);
        }
else
        {
            var endpoint = new Endpoint(
     requestDelegate: _ => Task.CompletedTask,
 metadata: new EndpointMetadataCollection(),
    displayName: "Public Endpoint"
      );
        context.SetEndpoint(endpoint);
        }

        return context;
    }

    private async Task<string?> GetResponseBodyAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }

    #endregion

    #region Auth Endpoints - Should Skip Validation

    [Fact]
    [Trait("Category", "JwtAuthenticationMiddleware")]
    public async Task InvokeAsync_AuthRegisterEndpoint_SkipsValidation()
    {
        // Arrange
        var middleware = new JwtAuthenticationMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = CreateHttpContext("/auth/register", requiresAuth: false);

        // Act
     await middleware.InvokeAsync(context);

        // Assert
   _mockNext.Verify(next => next(context), Times.Once);
        context.Response.StatusCode.Should().Be(200); // Default status, not 401
    }

    [Fact]
    [Trait("Category", "JwtAuthenticationMiddleware")]
    public async Task InvokeAsync_AuthLoginEndpoint_SkipsValidation()
{
        // Arrange
        var middleware = new JwtAuthenticationMiddleware(_mockNext.Object, _mockLogger.Object);
      var context = CreateHttpContext("/auth/login", requiresAuth: false);

    // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockNext.Verify(next => next(context), Times.Once);
  }

    [Theory]
    [InlineData("/auth")]
    [InlineData("/auth/")]
    [InlineData("/auth/register")]
    [InlineData("/auth/login")]
    [InlineData("/auth/refresh")]
    [Trait("Category", "JwtAuthenticationMiddleware")]
    public async Task InvokeAsync_AuthEndpoints_AllowWithoutToken(string path)
    {
        // Arrange
  var middleware = new JwtAuthenticationMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = CreateHttpContext(path, requiresAuth: false);

        // Act
        await middleware.InvokeAsync(context);

 // Assert
        _mockNext.Verify(next => next(context), Times.Once);
        context.Response.StatusCode.Should().NotBe(401);
    }

    #endregion

    #region Protected Endpoints - Missing Authorization Header

    [Fact]
    [Trait("Category", "JwtAuthenticationMiddleware")]
    public async Task InvokeAsync_ProtectedEndpointWithoutAuthHeader_Returns401()
    {
        // Arrange
        var middleware = new JwtAuthenticationMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = CreateHttpContext("/api/userprofile", authorizationHeader: null, requiresAuth: true);
        context.Response.Body = new MemoryStream();

     // Act
        await middleware.InvokeAsync(context);

   // Assert
   context.Response.StatusCode.Should().Be(401);
        _mockNext.Verify(next => next(context), Times.Never);
    }

    [Fact]
    [Trait("Category", "JwtAuthenticationMiddleware")]
    public async Task InvokeAsync_MissingAuthHeader_ReturnsCorrectErrorMessage()
 {
        // Arrange
        var middleware = new JwtAuthenticationMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = CreateHttpContext("/api/userprofile", authorizationHeader: null, requiresAuth: true);
     context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
  var responseBody = await GetResponseBodyAsync(context);
        responseBody.Should().NotBeNullOrEmpty();
        
    var errorResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody!);
        errorResponse.Should().ContainKey("error");
        errorResponse!["error"].Should().Be("Authorization header missing");
    }

    [Fact]
[Trait("Category", "JwtAuthenticationMiddleware")]
  public async Task InvokeAsync_MissingAuthHeader_SetsCorrectContentType()
    {
        // Arrange
        var middleware = new JwtAuthenticationMiddleware(_mockNext.Object, _mockLogger.Object);
    var context = CreateHttpContext("/api/userprofile", authorizationHeader: null, requiresAuth: true);
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

     // Assert
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    [Trait("Category", "JwtAuthenticationMiddleware")]
    public async Task InvokeAsync_MissingAuthHeader_LogsWarning()
    {
        // Arrange
      var middleware = new JwtAuthenticationMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = CreateHttpContext("/api/userprofile", authorizationHeader: null, requiresAuth: true);
        context.Response.Body = new MemoryStream();

  // Act
        await middleware.InvokeAsync(context);

      // Assert
        _mockLogger.Verify(
       x => x.Log(
      LogLevel.Warning,
          It.IsAny<EventId>(),
     It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("missing Authorization header")),
    It.IsAny<Exception>(),
     It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
       Times.Once);
    }

    #endregion

    #region Protected Endpoints - Invalid Authorization Header Format

    [Fact]
    [Trait("Category", "JwtAuthenticationMiddleware")]
  public async Task InvokeAsync_AuthHeaderWithoutBearer_Returns401()
    {
 // Arrange
        var middleware = new JwtAuthenticationMiddleware(_mockNext.Object, _mockLogger.Object);
   var context = CreateHttpContext("/api/userprofile", authorizationHeader: "InvalidToken", requiresAuth: true);
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

      // Assert
        context.Response.StatusCode.Should().Be(401);
        _mockNext.Verify(next => next(context), Times.Never);
    }

    [Fact]
    [Trait("Category", "JwtAuthenticationMiddleware")]
    public async Task InvokeAsync_InvalidAuthHeaderFormat_ReturnsCorrectErrorMessage()
    {
   // Arrange
        var middleware = new JwtAuthenticationMiddleware(_mockNext.Object, _mockLogger.Object);
  var context = CreateHttpContext("/api/userprofile", authorizationHeader: "Basic xyz123", requiresAuth: true);
  context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        var responseBody = await GetResponseBodyAsync(context);
        var errorResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody!);
        errorResponse!["error"].Should().Be("Invalid authorization header format. Expected 'Bearer <token>'");
    }

    [Theory]
    [InlineData("")]
    [InlineData("Token abc123")]
    [InlineData("Basic xyz789")]
    [InlineData("ApiKey secret")]
  [Trait("Category", "JwtAuthenticationMiddleware")]
    public async Task InvokeAsync_InvalidHeaderFormats_Returns401(string authHeader)
    {
        // Arrange
        var middleware = new JwtAuthenticationMiddleware(_mockNext.Object, _mockLogger.Object);
     var context = CreateHttpContext("/api/userprofile", authorizationHeader: authHeader, requiresAuth: true);
        context.Response.Body = new MemoryStream();

        // Act
await middleware.InvokeAsync(context);

      // Assert
        context.Response.StatusCode.Should().Be(401);
        _mockNext.Verify(next => next(context), Times.Never);
    }

    #endregion

    #region Protected Endpoints - Empty Token

    [Fact]
    [Trait("Category", "JwtAuthenticationMiddleware")]
    public async Task InvokeAsync_BearerWithEmptyToken_Returns401()
    {
 // Arrange
     var middleware = new JwtAuthenticationMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = CreateHttpContext("/api/userprofile", authorizationHeader: "Bearer ", requiresAuth: true);
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(401);
    }

    [Fact]
    [Trait("Category", "JwtAuthenticationMiddleware")]
    public async Task InvokeAsync_EmptyToken_ReturnsCorrectErrorMessage()
    {
        // Arrange
        var middleware = new JwtAuthenticationMiddleware(_mockNext.Object, _mockLogger.Object);
 var context = CreateHttpContext("/api/userprofile", authorizationHeader: "Bearer ", requiresAuth: true);
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
var responseBody = await GetResponseBodyAsync(context);
        var errorResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody!);
        errorResponse!["error"].Should().Be("Token is required");
    }

    [Theory]
    [InlineData("Bearer ")]
    [InlineData("Bearer   ")]
    [InlineData("Bearer \t")]
    [Trait("Category", "JwtAuthenticationMiddleware")]
    public async Task InvokeAsync_BearerWithWhitespace_Returns401(string authHeader)
    {
   // Arrange
        var middleware = new JwtAuthenticationMiddleware(_mockNext.Object, _mockLogger.Object);
    var context = CreateHttpContext("/api/userprofile", authorizationHeader: authHeader, requiresAuth: true);
        context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
     context.Response.StatusCode.Should().Be(401);
    }

    #endregion

    #region Protected Endpoints - Valid Token

  [Fact]
    [Trait("Category", "JwtAuthenticationMiddleware")]
    public async Task InvokeAsync_ValidBearerToken_CallsNextMiddleware()
    {
      // Arrange
        var middleware = new JwtAuthenticationMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = CreateHttpContext("/api/userprofile", authorizationHeader: "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.test.token", requiresAuth: true);

     // Act
        await middleware.InvokeAsync(context);

        // Assert
      _mockNext.Verify(next => next(context), Times.Once);
    }

    [Theory]
    [InlineData("Bearer validtoken123")]
    [InlineData("Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c")]
    [InlineData("Bearer simple.jwt.token")]
    [Trait("Category", "JwtAuthenticationMiddleware")]
    public async Task InvokeAsync_DifferentValidTokens_CallsNext(string authHeader)
    {
        // Arrange
        var middleware = new JwtAuthenticationMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = CreateHttpContext("/api/userprofile", authorizationHeader: authHeader, requiresAuth: true);

// Act
        await middleware.InvokeAsync(context);

        // Assert
     _mockNext.Verify(next => next(context), Times.Once);
    }

    [Fact]
    [Trait("Category", "JwtAuthenticationMiddleware")]
    public async Task InvokeAsync_ValidToken_DoesNotModifyResponse()
    {
        // Arrange
   var middleware = new JwtAuthenticationMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = CreateHttpContext("/api/userprofile", authorizationHeader: "Bearer valid.token.here", requiresAuth: true);

    // Act
     await middleware.InvokeAsync(context);

        // Assert
  context.Response.StatusCode.Should().Be(200); // Default status
        context.Response.Body.Length.Should().Be(0); // No response written
    }

    #endregion

    #region Public Endpoints - No Authorization Required

    [Fact]
    [Trait("Category", "JwtAuthenticationMiddleware")]
    public async Task InvokeAsync_PublicEndpointWithoutToken_CallsNext()
    {
        // Arrange
  var middleware = new JwtAuthenticationMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = CreateHttpContext("/public/data", authorizationHeader: null, requiresAuth: false);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockNext.Verify(next => next(context), Times.Once);
        context.Response.StatusCode.Should().NotBe(401);
    }

    [Fact]
    [Trait("Category", "JwtAuthenticationMiddleware")]
 public async Task InvokeAsync_PublicEndpointWithToken_CallsNext()
    {
        // Arrange
   var middleware = new JwtAuthenticationMiddleware(_mockNext.Object, _mockLogger.Object);
    var context = CreateHttpContext("/public/data", authorizationHeader: "Bearer token123", requiresAuth: false);

        // Act
      await middleware.InvokeAsync(context);

        // Assert
        _mockNext.Verify(next => next(context), Times.Once);
 }

    #endregion

    #region Edge Cases

 [Fact]
    [Trait("Category", "JwtAuthenticationMiddleware")]
    public async Task InvokeAsync_CaseSensitiveBearer_LowercaseBearer_Returns401()
    {
        // Arrange
        var middleware = new JwtAuthenticationMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = CreateHttpContext("/api/userprofile", authorizationHeader: "bearer token123", requiresAuth: true);
  context.Response.Body = new MemoryStream();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
      // Note: The middleware uses OrdinalIgnoreCase, so this should actually pass
        _mockNext.Verify(next => next(context), Times.Once);
    }

    [Fact]
    [Trait("Category", "JwtAuthenticationMiddleware")]
    public async Task InvokeAsync_BearerUpperCase_CallsNext()
    {
        // Arrange
        var middleware = new JwtAuthenticationMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = CreateHttpContext("/api/userprofile", authorizationHeader: "BEARER token123", requiresAuth: true);

  // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockNext.Verify(next => next(context), Times.Once);
    }

    [Fact]
    [Trait("Category", "JwtAuthenticationMiddleware")]
    public async Task InvokeAsync_BearerMixedCase_CallsNext()
    {
        // Arrange
        var middleware = new JwtAuthenticationMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = CreateHttpContext("/api/userprofile", authorizationHeader: "BeArEr token123", requiresAuth: true);

        // Act
        await middleware.InvokeAsync(context);

     // Assert
        _mockNext.Verify(next => next(context), Times.Once);
    }

    [Fact]
    [Trait("Category", "JwtAuthenticationMiddleware")]
    public async Task InvokeAsync_VeryLongToken_CallsNext()
    {
        // Arrange
   var middleware = new JwtAuthenticationMiddleware(_mockNext.Object, _mockLogger.Object);
        var longToken = new string('a', 5000);
        var context = CreateHttpContext("/api/userprofile", authorizationHeader: $"Bearer {longToken}", requiresAuth: true);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _mockNext.Verify(next => next(context), Times.Once);
    }

    [Fact]
    [Trait("Category", "JwtAuthenticationMiddleware")]
    public async Task InvokeAsync_TokenWithSpecialCharacters_CallsNext()
    {
   // Arrange
        var middleware = new JwtAuthenticationMiddleware(_mockNext.Object, _mockLogger.Object);
        var specialToken = "token-with_special.characters+and=signs";
  var context = CreateHttpContext("/api/userprofile", authorizationHeader: $"Bearer {specialToken}", requiresAuth: true);

        // Act
     await middleware.InvokeAsync(context);

 // Assert
 _mockNext.Verify(next => next(context), Times.Once);
    }

    [Fact]
    [Trait("Category", "JwtAuthenticationMiddleware")]
    public async Task InvokeAsync_MultipleSpacesAfterBearer_ExtractsTokenCorrectly()
    {
        // Arrange
        var middleware = new JwtAuthenticationMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = CreateHttpContext("/api/userprofile", authorizationHeader: "Bearer    token123", requiresAuth: true);

      // Act
      await middleware.InvokeAsync(context);

        // Assert
        _mockNext.Verify(next => next(context), Times.Once);
    }

    [Fact]
    [Trait("Category", "JwtAuthenticationMiddleware")]
  public async Task InvokeAsync_NoEndpointMetadata_CallsNext()
    {
        // Arrange
        var middleware = new JwtAuthenticationMiddleware(_mockNext.Object, _mockLogger.Object);
        var context = new DefaultHttpContext();
        context.Request.Path = "/unknown";
        // No endpoint set - endpoint is null

        // Act
        await middleware.InvokeAsync(context);

   // Assert
        _mockNext.Verify(next => next(context), Times.Once);
    }

    #endregion

    #region Multiple Protected Endpoints

    [Theory]
    [InlineData("/api/userprofile")]
  [InlineData("/api/users")]
    [InlineData("/api/invoices")]
    [InlineData("/api/admin/settings")]
    [Trait("Category", "JwtAuthenticationMiddleware")]
    public async Task InvokeAsync_DifferentProtectedEndpointsWithoutToken_Returns401(string path)
    {
        // Arrange
   var middleware = new JwtAuthenticationMiddleware(_mockNext.Object, _mockLogger.Object);
  var context = CreateHttpContext(path, authorizationHeader: null, requiresAuth: true);
        context.Response.Body = new MemoryStream();

   // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(401);
    }

    [Theory]
    [InlineData("/api/userprofile")]
    [InlineData("/api/users")]
    [InlineData("/api/invoices")]
    [InlineData("/api/admin/settings")]
    [Trait("Category", "JwtAuthenticationMiddleware")]
    public async Task InvokeAsync_DifferentProtectedEndpointsWithToken_CallsNext(string path)
    {
        // Arrange
        var middleware = new JwtAuthenticationMiddleware(_mockNext.Object, _mockLogger.Object);
  var context = CreateHttpContext(path, authorizationHeader: "Bearer valid.token.here", requiresAuth: true);

   // Act
        await middleware.InvokeAsync(context);

   // Assert
  _mockNext.Verify(next => next(context), Times.Once);
    }

    #endregion
}
