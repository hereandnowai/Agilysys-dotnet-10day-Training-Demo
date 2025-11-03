# Testing the /auth/register Endpoint

## Using curl

### Successful Registration
```bash
curl -X POST http://localhost:5000/auth/register \
  -H "Content-Type: application/json" \
  -d '{
 "name": "John Doe",
    "email": "john@example.com",
    "password": "SecurePassword123"
  }'
```

Expected Response (201 Created):
```json
{
  "userId": 1,
  "message": "User registered successfully"
}
```

### Duplicate Email (Should fail)
```bash
curl -X POST http://localhost:5000/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Jane Doe",
    "email": "john@example.com",
    "password": "AnotherPassword456"
  }'
```

Expected Response (409 Conflict):
```json
{
  "error": "Email already exists"
}
```

### Invalid Email Format
```bash
curl -X POST http://localhost:5000/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Invalid User",
"email": "not-an-email",
    "password": "Password123"
  }'
```

Expected Response (400 Bad Request):
```json
{
  "error": "Invalid email format"
}
```

### Missing Required Fields
```bash
curl -X POST http://localhost:5000/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test User"
  }'
```

Expected Response (400 Bad Request):
```json
{
  "error": "Email is required, Password is required"
}
```

### Password Too Short
```bash
curl -X POST http://localhost:5000/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test User",
    "email": "test@example.com",
    "password": "short"
  }'
```

Expected Response (400 Bad Request):
```json
{
  "error": "Password must be at least 8 characters long"
}
```

## Using PowerShell (Invoke-RestMethod)

### Successful Registration
```powershell
$body = @{
    name = "John Doe"
    email = "john@example.com"
    password = "SecurePassword123"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/auth/register" `
    -Method Post `
    -ContentType "application/json" `
    -Body $body
```

## Using Postman

1. Create a new POST request
2. URL: `http://localhost:5000/auth/register`
3. Headers: `Content-Type: application/json`
4. Body (raw JSON):
```json
{
  "name": "John Doe",
  "email": "john@example.com",
  "password": "SecurePassword123"
}
```

## Password Hashing Verification

The implementation uses BCrypt with a work factor of 12 for secure password hashing:
- Passwords are never stored in plain text
- Each password gets a unique salt
- The hash is computationally expensive to crack
- Password hashes are never returned in API responses

## Notes

- Replace `http://localhost:5000` with your actual API URL
- The User ID is auto-incremented starting from 1
- Email addresses are case-insensitive for uniqueness checks
- All user data is stored in-memory (will be lost on restart)
