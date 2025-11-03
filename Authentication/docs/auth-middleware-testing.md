# Testing Authentication Middleware

## Overview

This guide demonstrates how to test the JWT authentication middleware that protects endpoints and validates tokens.

## Protected vs Public Endpoints

### Public Endpoints (No Authentication Required)
- `POST /auth/register` - Register new user
- `POST /auth/login` - Login and get token
- `GET /api/invoice` - Get all invoices (demo)
- `GET /api/invoice/{id}` - Get single invoice (demo)

### Protected Endpoints (Authentication Required)
- `POST /api/invoice` - Create invoice
- `PUT /api/invoice/{id}` - Update invoice
- `DELETE /api/invoice/{id}` - Delete invoice
- `GET /api/userprofile` - Get user profile
- `GET /api/userprofile/claims` - Get all JWT claims
- `GET /api/userprofile/verify` - Verify authentication

## Test Scenarios

### 1. Missing Token (401 Unauthorized)

Test accessing a protected endpoint without a token:

```bash
curl -v -X GET http://localhost:5000/api/userprofile
```

**Expected Response:**
- Status: `401 Unauthorized`
- Body:
```json
{
  "error": "Authorization header missing"
}
```

### 2. Invalid Token Format (401 Unauthorized)

Test with invalid authorization header format:

```bash
curl -v -X GET http://localhost:5000/api/userprofile \
  -H "Authorization: InvalidFormat"
```

**Expected Response:**
- Status: `401 Unauthorized`
- Body:
```json
{
  "error": "Invalid or missing token"
}
```

### 3. Empty Token (401 Unauthorized)

Test with Bearer prefix but no token:

```bash
curl -v -X GET http://localhost:5000/api/userprofile \
  -H "Authorization: Bearer "
```

**Expected Response:**
- Status: `401 Unauthorized`
- Body:
```json
{
  "error": "Token is required"
}
```

### 4. Invalid Token (401 Unauthorized)

Test with malformed JWT token:

```bash
curl -v -X GET http://localhost:5000/api/userprofile \
  -H "Authorization: Bearer invalid.token.here"
```

**Expected Response:**
- Status: `401 Unauthorized`
- Body:
```json
{
  "error": "Invalid token"
}
```

### 5. Expired Token (401 Unauthorized)

After a token expires (default 60 minutes):

```bash
curl -v -X GET http://localhost:5000/api/userprofile \
  -H "Authorization: Bearer <expired_token>"
```

**Expected Response:**
- Status: `401 Unauthorized`
- Headers: `Token-Expired: true`
- Body:
```json
{
  "error": "Token expired"
}
```

### 6. Valid Token (200 OK)

Complete authentication flow:

```bash
# Step 1: Register a user
curl -X POST http://localhost:5000/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test User",
    "email": "test@example.com",
    "password": "TestPass123"
  }'

# Step 2: Login to get token
TOKEN=$(curl -s -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "TestPass123"
  }' | jq -r '.token')

echo "Token: $TOKEN"

# Step 3: Access protected endpoint with valid token
curl -v -X GET http://localhost:5000/api/userprofile \
  -H "Authorization: Bearer $TOKEN"
```

**Expected Response:**
- Status: `200 OK`
- Body:
```json
{
  "userId": "1",
  "email": "test@example.com",
  "name": "Test User",
  "tokenId": "unique-guid",
  "isAuthenticated": true,
  "authenticationType": "AuthenticationTypes.Federation"
}
```

## Testing All Protected Endpoints

### Get User Profile

```bash
curl -X GET http://localhost:5000/api/userprofile \
  -H "Authorization: Bearer $TOKEN"
```

**Success Response (200):**
```json
{
  "userId": "1",
  "email": "test@example.com",
  "name": "Test User",
  "tokenId": "guid",
  "isAuthenticated": true,
  "authenticationType": "AuthenticationTypes.Federation"
}
```

### Get All Claims

```bash
curl -X GET http://localhost:5000/api/userprofile/claims \
  -H "Authorization: Bearer $TOKEN"
```

**Success Response (200):**
```json
[
  { "type": "sub", "value": "1" },
  { "type": "email", "value": "test@example.com" },
{ "type": "name", "value": "Test User" },
  { "type": "jti", "value": "unique-guid" },
  { "type": "iat", "value": "1730285730" },
  { "type": "exp", "value": "1730289330" },
  { "type": "iss", "value": "InvoiceApi" },
  { "type": "aud", "value": "InvoiceApiUsers" }
]
```

### Verify Authentication

```bash
curl -X GET http://localhost:5000/api/userprofile/verify \
  -H "Authorization: Bearer $TOKEN"
```

**Success Response (200):**
```json
{
  "isAuthenticated": true,
  "userId": "1",
  "message": "Token is valid and user is authenticated"
}
```

### Create Invoice (Protected)

```bash
curl -X POST http://localhost:5000/api/invoice \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "customer": "Acme Corp",
    "amount": 1500.00,
    "status": "Pending"
  }'
```

**Success Response (201):**
```json
{
  "id": 1,
  "customer": "Acme Corp",
  "date": "2025-10-30T10:30:00Z",
  "amount": 1500.00,
  "status": "Pending"
}
```

**Without Token (401):**
```json
{
  "error": "Authorization header missing"
}
```

### Update Invoice (Protected)

```bash
curl -X PUT http://localhost:5000/api/invoice/1 \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "customer": "Acme Corp Updated",
    "amount": 2000.00,
    "status": "Approved"
  }'
```

**Success Response:** `204 No Content`

**Without Token (401):**
```json
{
"error": "Authorization header missing"
}
```

### Delete Invoice (Protected)

```bash
curl -X DELETE http://localhost:5000/api/invoice/1 \
  -H "Authorization: Bearer $TOKEN"
```

**Success Response:** `204 No Content`

**Without Token (401):**
```json
{
  "error": "Authorization header missing"
}
```

## PowerShell Testing

### Complete Test Flow

```powershell
# 1. Register
$registerBody = @{
    name = "PowerShell User"
    email = "ps@example.com"
    password = "PowerShell123"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/auth/register" `
    -Method Post `
    -ContentType "application/json" `
    -Body $registerBody

# 2. Login and get token
$loginBody = @{
    email = "ps@example.com"
  password = "PowerShell123"
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri "http://localhost:5000/auth/login" `
    -Method Post `
    -ContentType "application/json" `
    -Body $loginBody

$token = $loginResponse.token
Write-Host "Token: $token"

# 3. Create headers with token
$headers = @{
    "Authorization" = "Bearer $token"
}

# 4. Test protected endpoint - Get Profile
$profile = Invoke-RestMethod -Uri "http://localhost:5000/api/userprofile" `
    -Method Get `
    -Headers $headers

Write-Host "User Profile:" -ForegroundColor Green
$profile | ConvertTo-Json

# 5. Test protected endpoint - Get Claims
$claims = Invoke-RestMethod -Uri "http://localhost:5000/api/userprofile/claims" `
    -Method Get `
    -Headers $headers

Write-Host "User Claims:" -ForegroundColor Green
$claims | ConvertTo-Json

# 6. Test protected endpoint - Create Invoice
$invoiceBody = @{
    customer = "PowerShell Corp"
    amount = 3000.00
    status = "Draft"
} | ConvertTo-Json

$invoice = Invoke-RestMethod -Uri "http://localhost:5000/api/invoice" `
    -Method Post `
    -ContentType "application/json" `
    -Headers $headers `
    -Body $invoiceBody

Write-Host "Created Invoice:" -ForegroundColor Green
$invoice | ConvertTo-Json

# 7. Test without token (should fail)
try {
    Invoke-RestMethod -Uri "http://localhost:5000/api/userprofile" `
        -Method Get
} catch {
  Write-Host "Expected Error (No Token):" -ForegroundColor Yellow
    Write-Host $_.Exception.Message
}
```

## Middleware Behavior

### Request Flow

```
1. Client sends request
   ?
2. RequestLoggingMiddleware (logs request)
   ?
3. JwtAuthenticationMiddleware (pre-validation checks)
   - Skips /auth endpoints
   - Checks for Authorization header on protected endpoints
   - Validates Bearer format
   - Returns 401 if checks fail
   ?
4. Authentication Middleware (validates JWT)
   - Validates signature
   - Checks expiry
   - Validates issuer/audience
   - Extracts claims
   - Returns 401 if invalid
   ?
5. Authorization Middleware (checks [Authorize] attribute)
   - Checks if endpoint requires auth
   - Verifies user is authenticated
   - Returns 401 if not authorized
   ?
6. Controller Action (executes business logic)
   - Accesses User.Claims
   - Processes request
   - Returns response
```

### Error Response Format

All authentication errors return consistent JSON format:

```json
{
  "error": "Descriptive error message"
}
```

### Logging

The middleware logs authentication events:

```
[INF] Token validated successfully for user 1 accessing /api/userprofile
[WRN] Token expired for request to /api/userprofile
[WRN] Request to /api/userprofile missing Authorization header
[WRN] Authentication failed for request to /api/userprofile: Invalid signature
```

## Testing Checklist

- [ ] Access protected endpoint without token ? 401
- [ ] Access protected endpoint with invalid format ? 401
- [ ] Access protected endpoint with empty token ? 401
- [ ] Access protected endpoint with malformed token ? 401
- [ ] Access protected endpoint with expired token ? 401
- [ ] Access protected endpoint with valid token ? 200
- [ ] Access public endpoint without token ? 200
- [ ] User information accessible in controller
- [ ] Claims properly extracted from JWT
- [ ] Logging works for all scenarios

## Acceptance Criteria Verification

? **Protected endpoints return 401 when token missing**
- Test: Access `/api/userprofile` without Authorization header
- Expected: 401 with error message

? **Protected endpoints return 401 when token invalid**
- Test: Access `/api/userprofile` with malformed token
- Expected: 401 with error message

? **Protected endpoints return 200 when token valid**
- Test: Login, get token, access `/api/userprofile` with valid token
- Expected: 200 with user profile data

? **User ID attached to request context**
- Test: Access protected endpoint and check controller can read `User.Claims`
- Expected: User ID, email, and name accessible via claims

## Common Issues

### Issue: Still getting 401 with valid token
**Solution:**
1. Check token hasn't expired (default 60 minutes)
2. Verify token format: `Authorization: Bearer <token>`
3. Ensure JWT secret in appsettings.json matches the one used to generate token

### Issue: Token expired too quickly
**Solution:**
- Adjust `ExpiryInMinutes` in `appsettings.json`
- Login again to get a fresh token

### Issue: Can't access User.Claims in controller
**Solution:**
- Ensure `[Authorize]` attribute is on the controller or action
- Check middleware order: Authentication before Authorization
- Use `User.FindFirst("sub")` or `User.FindFirst(ClaimTypes.NameIdentifier)`

## Security Notes

- All authentication errors return 401 (not 403) for security
- Error messages are generic to prevent information disclosure
- Tokens are validated on every request
- ClockSkew is set to zero for precise expiry enforcement
- Failed authentication attempts are logged for monitoring

## Next Steps

With authentication middleware complete, you can:
1. Add role-based authorization
2. Implement refresh tokens
3. Add rate limiting for protected endpoints
4. Implement token blacklist for logout
5. Add custom authorization policies
