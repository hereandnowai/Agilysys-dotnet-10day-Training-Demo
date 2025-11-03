# Story 2.2 — Login & JWT issue

## Story Details

**Epic:** Authentication (simple token-based)  
**Story ID:** 2.2  
**Estimate:** 2 SP  
**Labels:** auth, security

## Description
Implement POST /auth/login to validate credentials and return JWT Bearer token. Token expiry configurable.

## Acceptance Criteria
- ✅ Valid credentials → 200 with token
- ✅ Invalid → 401

## Copilot Prompt

```
Create POST /auth/login that validates email/password, issues JWT signed with secret from config, returns { token }.
```

## Implementation Steps

1. **Copy the copilot prompt** above
2. **Paste into GitHub Copilot Chat** or your IDE
3. **Follow the generated suggestions** to implement:
   - Login endpoint controller
   - Credential validation logic
   - JWT token generation
   - Configuration for JWT settings
   - Password verification against hash
   - Response formatting
4. **Configure JWT settings** in appsettings.json
5. **Test the endpoint** with valid and invalid credentials
6. **Verify token generation** and validation

## Expected Deliverables

- [ ] POST /auth/login endpoint implemented
- [ ] JWT NuGet packages installed:
  - [ ] Microsoft.AspNetCore.Authentication.JwtBearer
  - [ ] System.IdentityModel.Tokens.Jwt
- [ ] JWT configuration in appsettings.json:
  - [ ] Secret key
  - [ ] Issuer
  - [ ] Audience
  - [ ] Token expiry time
- [ ] Login logic implementation:
  - [ ] Email/password validation
  - [ ] User lookup by email
  - [ ] Password verification with bcrypt
  - [ ] JWT token generation
- [ ] Response handling:
  - [ ] 200 OK with token for valid credentials
  - [ ] 401 Unauthorized for invalid credentials
  - [ ] Proper error messages
- [ ] Token claims configuration
- [ ] Unit tests for login functionality

## Technical Requirements

### JWT Configuration (appsettings.json)
```json
{
  "Jwt": {
    "Key": "YourSuperSecretKeyThatIsAtLeast256BitsLong",
    "Issuer": "YourAppName",
    "Audience": "YourAppUsers",
    "ExpiryInMinutes": 60
  }
}
```

### API Contract
**Request:**
```json
POST /auth/login
{
  "email": "john@example.com",
  "password": "SecurePassword123"
}
```

**Success Response (200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-10-30T11:15:30Z"
}
```

**Error Response (401):**
```json
{
  "error": "Invalid email or password"
}
```

### JWT Token Claims
- **sub** (subject): User ID
- **email**: User email
- **name**: User name
- **iat** (issued at): Token creation time
- **exp** (expiry): Token expiration time

## Security Considerations
- Use strong secret key (256+ bits)
- Implement rate limiting for login attempts
- Don't reveal whether email or password is incorrect
- Use secure token expiry times
- Consider refresh token implementation for longer sessions
- Store JWT secret in environment variables for production

## Dependencies
- Microsoft.AspNetCore.Authentication.JwtBearer
- System.IdentityModel.Tokens.Jwt
- BCrypt.Net-Next (for password verification)

## Configuration Setup
```csharp
// Program.cs or Startup.cs
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
        };
    });
```

## Notes
- This story builds upon Story 2.1 (User registration)
- JWT secret should be different in each environment
- Consider implementing login attempt tracking
- Token expiry should be configurable per environment
- Plan for token refresh mechanism in future iterations# 1. Register a user
curl -X POST http://localhost:5000/auth/register \
  -H "Content-Type: application/json" \
  -d '{"name":"Test User","email":"test@example.com","password":"TestPass123"}'

# 2. Login to get token
curl -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"TestPass123"}'

# Response:
# {
#   "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
#   "expiresAt": "2025-10-30T12:15:30Z"
# }
