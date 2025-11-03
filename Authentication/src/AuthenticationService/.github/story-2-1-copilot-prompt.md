# Story 2.1 — User model & registration endpoint

## Story Details

**Epic:** Authentication (simple token-based)  
**Story ID:** 2.1  
**Estimate:** 2 SP  
**Labels:** auth, api

## Description
Create a User model (email, password-hash, name). Implement POST /auth/register to register user (store hashed password).

## Acceptance Criteria
- ✅ Password stored hashed (bcrypt or equivalent)
- ✅ Return 201 with user id (no password)

## Copilot Prompt

```
Generate endpoint POST /auth/register that accepts name,email,password, hashes password with bcrypt, stores user, and returns 201 with user id.
```

## Implementation Steps

1. **Copy the copilot prompt** above
2. **Paste into GitHub Copilot Chat** or your IDE
3. **Follow the generated suggestions** to implement:
   - User model/entity with required fields
   - Password hashing using bcrypt
   - Registration endpoint controller
   - Data storage/repository layer
   - Input validation
   - Response formatting
4. **Configure bcrypt** with appropriate salt rounds
5. **Test the endpoint** with sample registration data
6. **Verify password hashing** and response format

## Expected Deliverables

- [ ] User model/entity created with fields:
  - [ ] Name (string)
  - [ ] Email (string, unique)
  - [ ] Password hash (string)
  - [ ] User ID (primary key)
- [ ] BCrypt NuGet package installed
- [ ] POST /auth/register endpoint implemented
- [ ] Password hashing logic implemented
- [ ] User storage/repository functionality
- [ ] Input validation for:
  - [ ] Required fields (name, email, password)
  - [ ] Email format validation
  - [ ] Password strength requirements
- [ ] Response handling:
  - [ ] 201 Created with user ID
  - [ ] Password excluded from response
  - [ ] Proper error responses (400, 409)
- [ ] Unit tests for registration logic

## Technical Requirements

### User Model Fields
```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### API Contract
**Request:**
```json
POST /auth/register
{
  "name": "John Doe",
  "email": "john@example.com",
  "password": "SecurePassword123"
}
```

**Response (201):**
```json
{
  "userId": 123,
  "message": "User registered successfully"
}
```

**Error Response (400):**
```json
{
  "error": "Email already exists"
}
```

## Security Considerations
- Use bcrypt with appropriate salt rounds (12+)
- Validate email uniqueness before registration
- Implement password strength requirements
- Sanitize input data
- Never return password or hash in responses

## Dependencies
- BCrypt.Net-Next or similar hashing library
- Entity Framework Core (for data storage)
- Data validation attributes/FluentValidation

## Notes
- This story establishes the foundation for authentication
- Email should be unique identifier for users
- Consider implementing email verification in future stories
- Password policies should be configurable