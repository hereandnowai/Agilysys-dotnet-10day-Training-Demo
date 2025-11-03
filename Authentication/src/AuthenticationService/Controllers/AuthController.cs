using Microsoft.AspNetCore.Mvc;
using AuthenticationService.DTOs;
using AuthenticationService.Models;
using AuthenticationService.Repositories;
using AuthenticationService.Services;
using BCrypt.Net;

namespace AuthenticationService.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserRepository userRepository, 
        IJwtService jwtService,
        ILogger<AuthController> logger)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user with hashed password
    /// </summary>
    /// <param name="request">Registration details including name, email, and password</param>
    /// <returns>User ID and success message</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
    try
        {
      // Validate model state
   if (!ModelState.IsValid)
            {
    var errors = string.Join(", ", ModelState.Values
.SelectMany(v => v.Errors)
         .Select(e => e.ErrorMessage));
      
      _logger.LogWarning("Registration validation failed: {Errors}", errors);
    return BadRequest(new ErrorResponse { Error = errors });
}

     // Check if email already exists
    var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
     {
             _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
    return Conflict(new ErrorResponse { Error = "Email already exists" });
            }

            // Hash the password using BCrypt with salt rounds of 12
         var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);

            // Create new user
   var user = new User
    {
  Name = request.Name,
    Email = request.Email,
      PasswordHash = passwordHash
   };

     // Save user to repository
            var createdUser = await _userRepository.CreateAsync(user);

            _logger.LogInformation("User registered successfully with ID: {UserId}", createdUser.Id);

            // Return success response (without password)
var response = new RegisterResponse
            {
      UserId = createdUser.Id,
                Message = "User registered successfully"
          };

       return CreatedAtAction(nameof(Register), new { id = createdUser.Id }, response);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error during user registration");
            return StatusCode(StatusCodes.Status500InternalServerError, 
      new ErrorResponse { Error = "An error occurred during registration" });
        }
}

    /// <summary>
    /// Login with email and password to receive a JWT token
    /// </summary>
    /// <param name="request">Login credentials including email and password</param>
    /// <returns>JWT token and expiry time</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
 {
 try
        {
            // Validate model state
            if (!ModelState.IsValid)
       {
             var errors = string.Join(", ", ModelState.Values
    .SelectMany(v => v.Errors)
          .Select(e => e.ErrorMessage));
     
          _logger.LogWarning("Login validation failed: {Errors}", errors);
    return BadRequest(new ErrorResponse { Error = errors });
         }

            // Find user by email
            var user = await _userRepository.GetByEmailAsync(request.Email);
  if (user == null)
            {
    _logger.LogWarning("Login attempt with non-existent email: {Email}", request.Email);
     return Unauthorized(new ErrorResponse { Error = "Invalid email or password" });
            }

  // Verify password
          var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
          if (!isPasswordValid)
            {
     _logger.LogWarning("Login attempt with invalid password for email: {Email}", request.Email);
     return Unauthorized(new ErrorResponse { Error = "Invalid email or password" });
          }

            // Generate JWT token
            var token = _jwtService.GenerateToken(user);
  var expiresAt = _jwtService.GetTokenExpiry();

          _logger.LogInformation("User {UserId} logged in successfully", user.Id);

var response = new LoginResponse
    {
                Token = token,
   ExpiresAt = expiresAt
     };

    return Ok(response);
        }
        catch (Exception ex)
     {
            _logger.LogError(ex, "Error during user login");
       return StatusCode(StatusCodes.Status500InternalServerError, 
             new ErrorResponse { Error = "An error occurred during login" });
        }
    }
}
