# Story 2.3 — Auth middleware

## Story Details

**Epic:** Authentication (simple token-based)  
**Story ID:** 2.3  
**Estimate:** 1 SP  
**Labels:** auth, infra

## Description
Authentication middleware to protect endpoints, read Authorization: Bearer <token>.

## Acceptance Criteria
- ✅ Protected endpoints return 401 when token missing/invalid; 200 when valid

## Copilot Prompt

```
Make middleware that verifies JWT from Authorization header and attaches user id to request context, returns 401 if invalid.
```

## Implementation Steps

1. **Copy the copilot prompt** above
2. **Paste into GitHub Copilot Chat** or your IDE
3. **Follow the generated suggestions** to implement:
   - JWT authentication middleware
   - Authorization header parsing
   - Token validation logic
   - User context attachment
   - Error handling for invalid tokens
4. **Configure middleware** in the request pipeline
5. **Add [Authorize] attributes** to protected endpoints
6. **Test protected endpoints** with and without valid tokens

## Expected Deliverables

- [ ] JWT authentication middleware configured
- [ ] Authorization header parsing implemented
- [ ] Token validation logic:
  - [ ] Bearer token extraction
  - [ ] JWT signature verification
  - [ ] Token expiry validation
  - [ ] Claims extraction
- [ ] User context attachment:
  - [ ] User ID accessible in controllers
  - [ ] User claims available in request context
- [ ] Error handling:
  - [ ] 401 for missing token
  - [ ] 401 for invalid token
  - [ ] 401 for expired token
- [ ] Controller protection:
  - [ ] [Authorize] attribute usage
  - [ ] Access to current user information
- [ ] Testing scenarios:
  - [ ] Valid token access
  - [ ] Invalid token rejection
  - [ ] Missing token rejection
  - [ ] Expired token rejection

## Technical Requirements

### Middleware Configuration
```csharp
// Program.cs
app.UseAuthentication();
app.UseAuthorization();
```

### Protected Controller Example
```csharp
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        var userId = User.FindFirst("sub")?.Value;
        var userEmail = User.FindFirst("email")?.Value;
        
        return Ok(new { userId, userEmail });
    }
}
```

### Authorization Header Format
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Error Responses
**Missing Token (401):**
```json
{
  "error": "Authorization header missing"
}
```

**Invalid Token (401):**
```json
{
  "error": "Invalid token"
}
```

**Expired Token (401):**
```json
{
  "error": "Token expired"
}
```

## Implementation Details

### JWT Configuration in Program.cs
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ClockSkew = TimeSpan.Zero
        };
    });
```

### Accessing User Information in Controllers
```csharp
// Get current user ID
var userId = int.Parse(User.FindFirst("sub").Value);

// Get current user email
var userEmail = User.FindFirst("email").Value;

// Check if user is authenticated
if (User.Identity.IsAuthenticated)
{
    // User is authenticated
}
```

## Security Considerations
- Validate token signature and expiry
- Use HTTPS in production to protect tokens
- Implement proper error handling without exposing sensitive information
- Consider token blacklisting for logout functionality
- Set appropriate clock skew for token validation

## Testing Scenarios

### Valid Token Test
```bash
curl -H "Authorization: Bearer valid_jwt_token" http://localhost:5000/api/users/profile
# Expected: 200 OK with user data
```

### Invalid Token Test
```bash
curl -H "Authorization: Bearer invalid_token" http://localhost:5000/api/users/profile
# Expected: 401 Unauthorized
```

### Missing Token Test
```bash
curl http://localhost:5000/api/users/profile
# Expected: 401 Unauthorized
```

# 1. Register
curl -X POST http://localhost:5000/auth/register \
  -H "Content-Type: application/json" \
  -d '{"name":"Test User","email":"test@example.com","password":"Test12345"}'

# 2. Login
TOKEN=$(curl -s -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test12345"}' | jq -r '.token')

# 3. Test WITHOUT token (should fail with 401)
curl -v http://localhost:5000/api/userprofile
# Response: 401 {"error":"Authorization header missing"}

# 4. Test WITH valid token (should succeed)
curl -v http://localhost:5000/api/userprofile \
  -H "Authorization: Bearer $TOKEN"
# Response: 200 OK with user profile data


## Dependencies
- Microsoft.AspNetCore.Authentication.JwtBearer
- System.IdentityModel.Tokens.Jwt

## Notes
- This story completes the basic authentication flow
- Builds upon Stories 2.1 and 2.2
- All protected endpoints will require valid JWT tokens
- Consider implementing role-based authorization in future stories
- Middleware order is important: Authentication before Authorization

Get-Content "src\InvoiceApi\logs\app.log" -Tail 50 -Wait