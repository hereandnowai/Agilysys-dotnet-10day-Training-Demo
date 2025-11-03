# Authentication Quick Start Guide

## Overview

This guide provides a quick walkthrough of the authentication system implemented in Stories 2.1 and 2.2.

## Architecture

```
???????????????
?   Client    ?
???????????????
       ?
       ? 1. Register
       ?
???????????????????????????????????
?  POST /auth/register    ?
?  - Validates input      ?
?  - Hashes password (BCrypt)   ?
?  - Stores user           ?
?  - Returns user ID     ?
???????????????????????????????????
    ?
       ? 2. Login
      ?
???????????????????????????????????
?  POST /auth/login     ?
?  - Validates credentials        ?
?  - Verifies password (BCrypt)   ?
?  - Generates JWT token          ?
?  - Returns token + expiry       ?
???????????????????????????????????
       ?
              ? 3. Use Token
              ?
???????????????????????????????????
?  Protected Endpoints   ?
?  - Validates JWT token ?
?  - Extracts user claims     ?
?  - Authorizes request           ?
?  - Returns data  ?
???????????????????????????????????
```

## Quick Test (5 Minutes)

### 1. Start the Application

```bash
cd src/InvoiceApi
dotnet run
```

The API will start on `http://localhost:5000` (or check the console output)

### 2. Register a User

```bash
curl -X POST http://localhost:5000/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test User",
    "email": "test@example.com",
    "password": "TestPassword123"
  }'
```

? **Expected Response (201 Created):**
```json
{
  "userId": 1,
  "message": "User registered successfully"
}
```

### 3. Login to Get Token

```bash
curl -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "TestPassword123"
  }'
```

? **Expected Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-10-30T12:15:30Z"
}
```

### 4. Test Invalid Login

```bash
curl -X POST http://localhost:5000/auth/login \
-H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "WrongPassword"
  }'
```

? **Expected Response (401 Unauthorized):**
```json
{
  "error": "Invalid email or password"
}
```

## Using the Token

### Save the Token (Bash)

```bash
TOKEN=$(curl -s -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"TestPassword123"}' \
  | jq -r '.token')

echo "Token: $TOKEN"
```

### Save the Token (PowerShell)

```powershell
$response = Invoke-RestMethod -Uri "http://localhost:5000/auth/login" `
    -Method Post `
    -ContentType "application/json" `
    -Body '{"email":"test@example.com","password":"TestPassword123"}'

$token = $response.token
Write-Host "Token: $token"
```

### Use Token in Requests

```bash
curl -X GET http://localhost:5000/api/protected-endpoint \
  -H "Authorization: Bearer $TOKEN"
```

## Configuration

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

**Important:** In production, move the JWT Key to environment variables:

```bash
# Linux/Mac
export JWT__KEY="your-production-secret-key"

# Windows
set JWT__KEY=your-production-secret-key

# Or use appsettings.Production.json
```

## API Endpoints

### Authentication Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/auth/register` | Register new user | No |
| POST | `/auth/login` | Login and get JWT token | No |

### Request/Response Formats

#### Register
**Request:**
```json
{
  "name": "string",
  "email": "string (email format)",
  "password": "string (min 8 chars)"
}
```

**Response (201):**
```json
{
  "userId": 1,
  "message": "User registered successfully"
}
```

#### Login
**Request:**
```json
{
  "email": "string (email format)",
  "password": "string"
}
```

**Response (200):**
```json
{
  "token": "string (JWT token)",
  "expiresAt": "2025-10-30T12:15:30Z"
}
```

## Token Details

### Token Expiry
- Default: **60 minutes**
- Configurable in `appsettings.json`
- Production recommendation: **15-30 minutes**

### Token Claims

| Claim | Description |
|-------|-------------|
| `sub` | User ID |
| `email` | User email address |
| `name` | User full name |
| `jti` | Unique token identifier |
| `iat` | Issued at timestamp |
| `exp` | Expiry timestamp |
| `iss` | Issuer (InvoiceApi) |
| `aud` | Audience (InvoiceApiUsers) |

### Decode Your Token

Visit [jwt.io](https://jwt.io) and paste your token to see the claims.

## Protecting Endpoints

### Add [Authorize] Attribute

```csharp
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
[Authorize]  // ?? All endpoints require authentication
public class InvoiceController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Invoice>> GetAll() 
    {
        // Only authenticated users can access
 }

  [AllowAnonymous]  // ? This specific endpoint is public
    [HttpGet("public")]
    public ActionResult<string> GetPublicInfo()
    {
     return Ok("Public information");
    }
}
```

### Access User Information

```csharp
[HttpPost]
[Authorize]
public ActionResult Create(Invoice invoice)
{
    // Get current user's ID from JWT
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
    var userName = User.FindFirst(ClaimTypes.Name)?.Value;

    // Use user information
    invoice.CreatedBy = userId;
}
```

## Security Features

### Password Security
- ? BCrypt hashing with work factor 12
- ? Passwords never stored in plain text
- ? Passwords never returned in responses
- ? Secure verification using constant-time comparison

### Token Security
- ? HMAC SHA-256 signing algorithm
- ? 256+ bit secret key
- ? Configurable expiry time
- ? Stateless authentication
- ? Claims-based identity

### API Security
- ? Input validation on all endpoints
- ? Email format validation
- ? Password length requirements
- ? Generic error messages (prevents enumeration)
- ? Proper HTTP status codes

## Common Issues

### ? "Invalid email or password"
**Problem:** Wrong credentials
**Solution:** Double-check email and password, ensure user is registered

### ? 401 Unauthorized on protected endpoint
**Problem:** Missing or invalid token
**Solution:** 
- Ensure you're sending the token in Authorization header
- Format: `Authorization: Bearer <token>`
- Check token hasn't expired

### ? "Email already exists"
**Problem:** Attempting to register with existing email
**Solution:** Use a different email or login instead

### ? Token expired
**Problem:** Token lifetime exceeded
**Solution:** Login again to get a new token

## Testing Checklist

- [ ] Register a new user successfully
- [ ] Register with duplicate email (should fail)
- [ ] Register with invalid email format (should fail)
- [ ] Register with short password (should fail)
- [ ] Login with valid credentials
- [ ] Login with invalid password (should fail)
- [ ] Login with non-existent email (should fail)
- [ ] Use token to access protected endpoint
- [ ] Access protected endpoint without token (should fail)
- [ ] Decode JWT token at jwt.io
- [ ] Verify token expiry

## Complete Example Flow

```bash
# 1. Register
curl -X POST http://localhost:5000/auth/register \
  -H "Content-Type: application/json" \
  -d '{"name":"Alice","email":"alice@example.com","password":"Alice12345"}'

# Expected: {"userId":1,"message":"User registered successfully"}

# 2. Login
TOKEN=$(curl -s -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"alice@example.com","password":"Alice12345"}' \
  | jq -r '.token')

echo "Token: $TOKEN"

# 3. Use token for protected requests
curl -X GET http://localhost:5000/api/invoice \
  -H "Authorization: Bearer $TOKEN"

# 4. Create invoice (protected endpoint)
curl -X POST http://localhost:5000/api/invoice \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"customer":"Acme Corp","amount":1500.00,"status":"Pending"}'
```

## File Structure

```
src/InvoiceApi/
??? Controllers/
?   ??? AuthController.cs  # Registration & Login
?   ??? InvoiceController.cs   # Example protected endpoints
??? Models/
?   ??? User.cs        # User entity
?   ??? Invoice.cs          # Invoice entity
??? DTOs/
?   ??? AuthDtos.cs       # Request/Response DTOs
??? Services/
?   ??? JwtService.cs      # JWT token generation
??? Repositories/
?   ??? UserRepository.cs          # User data access
??? Configuration/
?   ??? JwtSettings.cs        # JWT configuration
??? Middleware/
?   ??? RequestLoggingMiddleware.cs
??? Program.cs                 # App configuration
??? appsettings.json              # JWT settings

docs/
??? auth-register-testing.md      # Registration testing guide
??? auth-login-testing.md     # Login testing guide
??? protecting-endpoints-with-jwt.md
??? story-2-1-implementation.md   # Story 2.1 summary
??? story-2-2-implementation.md   # Story 2.2 summary
??? authentication-quick-start.md # This file
```

## Next Steps

1. **Add Database:** Replace in-memory storage with Entity Framework Core
2. **Implement Refresh Tokens:** For better UX with short-lived access tokens
3. **Add Role-Based Authorization:** Implement user roles and permissions
4. **Email Verification:** Add email verification flow
5. **Password Reset:** Implement forgot password functionality
6. **Rate Limiting:** Add rate limiting to prevent brute force attacks
7. **Logout:** Implement token blacklist for logout functionality

## Additional Resources

- [JWT.io](https://jwt.io) - Decode and inspect JWT tokens
- [BCrypt Calculator](https://bcrypt-generator.com/) - Test BCrypt hashing
- [RFC 7519](https://tools.ietf.org/html/rfc7519) - JWT Specification
- [ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security/)

## Support

For detailed testing scenarios:
- Registration: `docs/auth-register-testing.md`
- Login: `docs/auth-login-testing.md`
- Protected Endpoints: `docs/protecting-endpoints-with-jwt.md`

For implementation details:
- Story 2.1: `docs/story-2-1-implementation.md`
- Story 2.2: `docs/story-2-2-implementation.md`
