# Testing the /auth/login Endpoint

## Prerequisites
First, register a user using the `/auth/register` endpoint:

```bash
curl -X POST http://localhost:5000/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "John Doe",
    "email": "john@example.com",
    "password": "SecurePassword123"
  }'
```

## Using curl

### Successful Login
```bash
curl -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password": "SecurePassword123"
  }'
```

Expected Response (200 OK):
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwiZW1haWwiOiJqb2huQGV4YW1wbGUuY29tIiwibmFtZSI6IkpvaG4gRG9lIiwianRpIjoiYTFiMmMzZDQtZTVmNi03OGg5LWkwajEtazJsMzRtNW42bzciLCJpYXQiOiIxNzMwMjg1NzMwIiwiZXhwIjoxNzMwMjg5MzMwLCJpc3MiOiJJbnZvaWNlQXBpIiwiYXVkIjoiSW52b2ljZUFwaVVzZXJzIn0.example_signature",
  "expiresAt": "2025-10-30T12:15:30Z"
}
```

### Invalid Password
```bash
curl -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password": "WrongPassword"
  }'
```

Expected Response (401 Unauthorized):
```json
{
  "error": "Invalid email or password"
}
```

### Non-existent Email
```bash
curl -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "nonexistent@example.com",
    "password": "SomePassword123"
  }'
```

Expected Response (401 Unauthorized):
```json
{
  "error": "Invalid email or password"
}
```

### Missing Required Fields
```bash
curl -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com"
  }'
```

Expected Response (400 Bad Request):
```json
{
  "error": "Password is required"
}
```

### Invalid Email Format
```bash
curl -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "not-an-email",
    "password": "SecurePassword123"
  }'
```

Expected Response (400 Bad Request):
```json
{
  "error": "Invalid email format"
}
```

## Using PowerShell (Invoke-RestMethod)

### Successful Login
```powershell
$loginBody = @{
    email = "john@example.com"
    password = "SecurePassword123"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:5000/auth/login" `
    -Method Post `
    -ContentType "application/json" `
    -Body $loginBody

# Store the token for later use
$token = $response.token
Write-Host "Token: $token"
Write-Host "Expires At: $($response.expiresAt)"
```

### Using the Token in Subsequent Requests
```powershell
# Example: Call a protected endpoint with the token
$headers = @{
    "Authorization" = "Bearer $token"
}

Invoke-RestMethod -Uri "http://localhost:5000/api/protected-resource" `
    -Method Get `
    -Headers $headers
```

## Using Postman

### Login Request
1. Create a new POST request
2. URL: `http://localhost:5000/auth/login`
3. Headers: `Content-Type: application/json`
4. Body (raw JSON):
```json
{
  "email": "john@example.com",
  "password": "SecurePassword123"
}
```
5. Send the request
6. Copy the token from the response

### Using Token in Postman
1. Go to the Authorization tab
2. Select Type: "Bearer Token"
3. Paste the token in the Token field
4. Send requests to protected endpoints

## Decoding the JWT Token

You can decode the JWT token at [jwt.io](https://jwt.io) to inspect its contents:

**Header:**
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

**Payload:**
```json
{
  "sub": "1",
  "email": "john@example.com",
  "name": "John Doe",
  "jti": "unique-token-id",
  "iat": 1730285730,
  "exp": 1730289330,
  "iss": "InvoiceApi",
  "aud": "InvoiceApiUsers"
}
```

**Claims Explanation:**
- `sub` (Subject): User ID
- `email`: User's email address
- `name`: User's full name
- `jti` (JWT ID): Unique token identifier
- `iat` (Issued At): Token creation timestamp
- `exp` (Expiry): Token expiration timestamp
- `iss` (Issuer): Token issuer (InvoiceApi)
- `aud` (Audience): Intended audience (InvoiceApiUsers)

## Token Configuration

The JWT token is configured in `appsettings.json`:

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

- **Default Token Expiry:** 60 minutes (1 hour)
- **Algorithm:** HMAC SHA-256 (HS256)
- **Key Length:** 256+ bits for security

## Security Features

1. **Password Verification:** Uses BCrypt to verify passwords against stored hashes
2. **Generic Error Messages:** Doesn't reveal whether email or password is incorrect
3. **Token Expiry:** Tokens automatically expire after configured time
4. **Secure Signing:** Tokens are signed with a secret key
5. **Claims-Based Identity:** Token contains user information for authorization

## Testing Full Authentication Flow

### 1. Register a New User
```bash
curl -X POST http://localhost:5000/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Jane Smith",
    "email": "jane@example.com",
    "password": "MySecurePass456"
  }'
```

### 2. Login to Get Token
```bash
TOKEN=$(curl -s -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "jane@example.com",
    "password": "MySecurePass456"
  }' | jq -r '.token')

echo "Token: $TOKEN"
```

### 3. Use Token for Protected Requests
```bash
curl -X GET http://localhost:5000/api/invoices \
  -H "Authorization: Bearer $TOKEN"
```

## Common Issues and Troubleshooting

### Issue: "Invalid email or password"
- **Solution:** Verify the email exists by registering first
- **Solution:** Double-check the password is correct

### Issue: Token expired
- **Solution:** Login again to get a new token
- **Solution:** Adjust `ExpiryInMinutes` in appsettings.json for longer sessions

### Issue: 401 Unauthorized on protected endpoints
- **Solution:** Ensure you're sending the token in the Authorization header
- **Solution:** Verify token hasn't expired
- **Solution:** Check the token format: `Bearer <token>`

## Production Considerations

?? **Important for Production:**

1. **Secret Key:** 
   - Store in environment variables or Azure Key Vault
   - Never commit to source control
   - Use different keys per environment

2. **HTTPS Only:** 
   - Always use HTTPS in production
   - Tokens sent over HTTP can be intercepted

3. **Token Expiry:**
   - Consider shorter expiry times (15-30 minutes)
   - Implement refresh tokens for better UX

4. **Rate Limiting:**
   - Implement rate limiting on login endpoint
   - Prevent brute force attacks

5. **Logging:**
   - Log failed login attempts
   - Monitor for suspicious activity
   - Don't log passwords or tokens

## Notes

- Tokens are stateless - the server doesn't store them
- Tokens cannot be revoked before expiry (consider implementing token blacklist if needed)
- The same user can have multiple valid tokens simultaneously
- Token expiry uses UTC time
