# JWT Authentication Quick Reference

## ?? Quick Test Commands

### 1. Register User
```bash
curl -X POST http://localhost:5000/auth/register \
  -H "Content-Type: application/json" \
  -d '{"name":"Dev User","email":"dev@example.com","password":"Dev12345"}'
```

### 2. Get Token
```bash
TOKEN=$(curl -s -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"dev@example.com","password":"Dev12345"}' \
  | jq -r '.token')
```

### 3. Use Token
```bash
curl -H "Authorization: Bearer $TOKEN" http://localhost:5000/api/userprofile
```

## ?? Endpoint Reference

### Public Endpoints
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/auth/register` | Register new user |
| POST | `/auth/login` | Get JWT token |
| GET | `/api/invoice` | List invoices |
| GET | `/api/invoice/{id}` | Get invoice |

### Protected Endpoints
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/invoice` | Create invoice |
| PUT | `/api/invoice/{id}` | Update invoice |
| DELETE | `/api/invoice/{id}` | Delete invoice |
| GET | `/api/userprofile` | Get user profile |
| GET | `/api/userprofile/claims` | Get JWT claims |
| GET | `/api/userprofile/verify` | Verify token |

## ??? Protecting Endpoints

### Protect Entire Controller
```csharp
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MyController : ControllerBase
{
// All actions require authentication
}
```

### Protect Specific Action
```csharp
[HttpPost]
[Authorize]
public IActionResult Create()
{
    // Only this action requires authentication
}
```

### Make Action Public in Protected Controller
```csharp
[Authorize]
public class MyController : ControllerBase
{
    [AllowAnonymous]// This action is public
    [HttpGet("public")]
    public IActionResult GetPublic() { }
}
```

## ?? Accessing User Information

```csharp
// Get User ID
var userId = User.FindFirst("sub")?.Value;

// Get Email
var email = User.FindFirst("email")?.Value;

// Get Name
var name = User.FindFirst("name")?.Value;

// Check if authenticated
if (User.Identity?.IsAuthenticated == true)
{
    // User is logged in
}

// Get all claims
var claims = User.Claims.Select(c => new { c.Type, c.Value });
```

## ? Error Responses

| Error | Status | Response |
|-------|--------|----------|
| No token | 401 | `{"error":"Authorization header missing"}` |
| Invalid format | 401 | `{"error":"Invalid or missing token"}` |
| Empty token | 401 | `{"error":"Token is required"}` |
| Bad token | 401 | `{"error":"Invalid token"}` |
| Expired token | 401 | `{"error":"Token expired"}` |

## ?? Configuration

### appsettings.json
```json
{
  "Jwt": {
    "Key": "YourSuperSecretKeyThatIsAtLeast256BitsLongForJWTTokenGeneration",
    "Issuer": "InvoiceApi",
    "Audience": "InvoiceApiUsers",
    "ExpiryInMinutes": 60
}
}
```

### Environment Variables (Production)
```bash
export JWT__KEY="your-production-secret"
export JWT__EXPIRY_IN_MINUTES="30"
```

## ?? Testing Checklist

- [ ] Register new user
- [ ] Login with valid credentials
- [ ] Login with invalid credentials (should fail)
- [ ] Access protected endpoint without token (should fail)
- [ ] Access protected endpoint with invalid token (should fail)
- [ ] Access protected endpoint with valid token (should succeed)
- [ ] Access public endpoint without token (should succeed)
- [ ] Verify user information in controller
- [ ] Test token expiry

## ?? Troubleshooting

### "Authorization header missing"
? Add header: `Authorization: Bearer <token>`

### "Invalid token"
? Check token format and validity
? Ensure JWT secret matches configuration

### "Token expired"
? Login again to get fresh token
? Check `ExpiryInMinutes` in configuration

### Can't access User.Claims
? Add `[Authorize]` attribute
? Check middleware order in Program.cs
? Verify token is valid

## ?? Required Packages

```xml
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.10" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.14.0" />
```

## ?? Security Checklist

- [ ] Use HTTPS in production
- [ ] Store JWT secret in environment variables
- [ ] Use strong JWT secret (256+ bits)
- [ ] Set appropriate token expiry (15-30 min recommended)
- [ ] Never log tokens or passwords
- [ ] Implement rate limiting on auth endpoints
- [ ] Monitor failed authentication attempts

## ?? Full Documentation

- **Quick Start:** `docs/authentication-quick-start.md`
- **Registration Testing:** `docs/auth-register-testing.md`
- **Login Testing:** `docs/auth-login-testing.md`
- **Middleware Testing:** `docs/auth-middleware-testing.md`
- **Protecting Endpoints:** `docs/protecting-endpoints-with-jwt.md`

## ?? Complete Test Flow

```bash
# Register
curl -X POST http://localhost:5000/auth/register \
  -H "Content-Type: application/json" \
  -d '{"name":"Test","email":"test@example.com","password":"Test12345"}'

# Login
TOKEN=$(curl -s -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test12345"}' | jq -r '.token')

# Get profile
curl -H "Authorization: Bearer $TOKEN" http://localhost:5000/api/userprofile

# Create invoice
curl -X POST http://localhost:5000/api/invoice \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"customer":"Acme","amount":1000,"status":"Draft"}'
```

## ?? PowerShell Quick Test

```powershell
# Login
$token = (Invoke-RestMethod -Uri "http://localhost:5000/auth/login" `
  -Method Post -ContentType "application/json" `
  -Body '{"email":"test@example.com","password":"Test12345"}').token

# Use token
$profile = Invoke-RestMethod -Uri "http://localhost:5000/api/userprofile" `
  -Method Get -Headers @{"Authorization"="Bearer $token"}

$profile | ConvertTo-Json
```

## ?? JWT Token Structure

```
Header (Base64):
{
  "alg": "HS256",
  "typ": "JWT"
}

Payload (Base64):
{
  "sub": "1",              // User ID
  "email": "user@example.com",
  "name": "User Name",
  "jti": "unique-id",    // Token ID
  "iat": 1730285730,       // Issued at
  "exp": 1730289330,       // Expires at
  "iss": "InvoiceApi",   // Issuer
  "aud": "InvoiceApiUsers" // Audience
}

Signature (HMAC SHA-256):
HMACSHA256(
  base64UrlEncode(header) + "." + base64UrlEncode(payload),
  secret
)
```

## ?? Useful Tools

- [jwt.io](https://jwt.io) - Decode and debug JWT tokens
- [Postman](https://www.postman.com/) - API testing
- [curl](https://curl.se/) - Command-line testing
- [jq](https://stedolan.github.io/jq/) - JSON parsing

---

**Story 2.3 Complete** ?  
Authentication middleware is fully configured and tested!
