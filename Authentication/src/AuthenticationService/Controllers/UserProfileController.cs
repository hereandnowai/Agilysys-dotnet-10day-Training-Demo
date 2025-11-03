using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthenticationService.Controllers;

/// <summary>
/// User profile management endpoints (requires authentication)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class UserProfileController : ControllerBase
{
    private readonly ILogger<UserProfileController> _logger;

    public UserProfileController(ILogger<UserProfileController> logger)
    {
    _logger = logger;
    }

    /// <summary>
    /// Get the current authenticated user's profile information
    /// </summary>
    /// <remarks>
    /// Extracts user information from the JWT token claims.
    /// Requires a valid JWT token in the Authorization header.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/userprofile
    ///     Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
    /// 
    /// </remarks>
    /// <returns>User profile information extracted from JWT claims</returns>
    /// <response code="200">Returns the user's profile information</response>
    /// <response code="401">If the token is missing, invalid, or expired</response>
    [HttpGet]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  public IActionResult GetProfile()
    {
        // Extract claims from the JWT token
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
      User.FindFirst("sub")?.Value;
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? 
    User.FindFirst("email")?.Value;
 var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? 
     User.FindFirst("name")?.Value;
      var tokenId = User.FindFirst("jti")?.Value;

   _logger.LogInformation("User profile accessed by user {UserId}", userId);

     var response = new UserProfileResponse
        {
     UserId = userId ?? "Unknown",
      Email = userEmail ?? "Unknown",
          Name = userName ?? "Unknown",
            TokenId = tokenId ?? "Unknown",
        IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
    AuthenticationType = User.Identity?.AuthenticationType ?? "Unknown"
     };

        return Ok(response);
}

    /// <summary>
    /// Get all JWT claims from the current token
    /// </summary>
    /// <remarks>
    /// Returns all claims embedded in the JWT token including standard and custom claims.
  /// Useful for debugging and understanding token contents.
    /// 
/// Sample request:
    /// 
    ///     GET /api/userprofile/claims
    ///     Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
    /// 
    /// </remarks>
    /// <returns>List of all claims in the JWT token</returns>
    /// <response code="200">Returns all claims from the token</response>
    /// <response code="401">If the token is missing, invalid, or expired</response>
    [HttpGet("claims")]
    [ProducesResponseType(typeof(List<ClaimInfo>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetClaims()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
            User.FindFirst("sub")?.Value;

        _logger.LogInformation("Claims accessed by user {UserId}", userId);

  var claims = User.Claims.Select(c => new ClaimInfo
 {
       Type = c.Type,
Value = c.Value
        }).ToList();

        return Ok(claims);
    }

    /// <summary>
 /// Verify that the JWT token is valid
    /// </summary>
    /// <remarks>
    /// Simple endpoint to verify if the provided JWT token is valid and the user is authenticated.
    /// Returns authentication status and user information.
    /// 
    /// Sample request:
 /// 
    ///     GET /api/userprofile/verify
    ///     Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
    /// 
    /// </remarks>
    /// <returns>Authentication verification result</returns>
    /// <response code="200">Token is valid and user is authenticated</response>
    /// <response code="401">If the token is missing, invalid, or expired</response>
    [HttpGet("verify")]
    [ProducesResponseType(typeof(AuthVerificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Verify()
    {
   var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
       User.FindFirst("sub")?.Value;

    _logger.LogInformation("Authentication verified for user {UserId}", userId);

        var response = new AuthVerificationResponse
        {
     IsAuthenticated = true,
            UserId = userId ?? "Unknown",
            Message = "Token is valid and user is authenticated"
        };

        return Ok(response);
    }
}

// Response DTOs

/// <summary>
/// User profile information response
/// </summary>
public class UserProfileResponse
{
    /// <summary>
    /// Unique user identifier
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// User's email address
 /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// User's full name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Unique token identifier (JTI claim)
    /// </summary>
    public string TokenId { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the user is authenticated
    /// </summary>
    public bool IsAuthenticated { get; set; }
    
    /// <summary>
    /// Authentication type/scheme used
    /// </summary>
    public string AuthenticationType { get; set; } = string.Empty;
}

/// <summary>
/// JWT claim information
/// </summary>
public class ClaimInfo
{
    /// <summary>
    /// Claim type/name
    /// </summary>
public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Claim value
    /// </summary>
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// Authentication verification response
/// </summary>
public class AuthVerificationResponse
{
    /// <summary>
    /// Whether the user is authenticated
    /// </summary>
    public bool IsAuthenticated { get; set; }

    /// <summary>
    /// User identifier
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Verification message
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
