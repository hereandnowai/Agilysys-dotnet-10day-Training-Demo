using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using AuthenticationService.Configuration;
using AuthenticationService.Models;

namespace AuthenticationService.Services;

public interface IJwtService
{
    string GenerateToken(User user);
    DateTime GetTokenExpiry();
}

public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<JwtService> _logger;

    public JwtService(JwtSettings jwtSettings, ILogger<JwtService> logger)
    {
        _jwtSettings = jwtSettings;
        _logger = logger;
}

    public string GenerateToken(User user)
    {
      try
        {
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

  var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
             new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.Name),
          new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
         new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

  var token = new JwtSecurityToken(
         issuer: _jwtSettings.Issuer,
  audience: _jwtSettings.Audience,
                claims: claims,
      expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
   signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            _logger.LogInformation("JWT token generated for user {UserId}", user.Id);
    
    return tokenString;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating JWT token for user {UserId}", user.Id);
            throw;
     }
    }

    public DateTime GetTokenExpiry()
    {
   return DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes);
    }
}
