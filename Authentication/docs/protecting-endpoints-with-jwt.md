# Protecting Endpoints with JWT Authentication

## Overview

Now that JWT authentication is configured, you can protect any endpoint by adding the `[Authorize]` attribute. This requires clients to send a valid JWT token in the Authorization header.

## Example: Protecting the Invoice Endpoints

### Option 1: Protect All Endpoints in a Controller

Add `[Authorize]` to the controller class to protect all endpoints:

```csharp
using InvoiceApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]  // ?? All endpoints in this controller now require authentication
public class InvoiceController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<Invoice>> GetAll() => Ok(Invoices);

 [HttpPost]
    public ActionResult<Invoice> Create(Invoice invoice) 
  {
        // Only authenticated users can create invoices
    }
}
```

### Option 2: Protect Specific Endpoints

Add `[Authorize]` to individual endpoints:

```csharp
namespace InvoiceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoiceController : ControllerBase
{
    [HttpGet]  // ? Public - anyone can view
  public ActionResult<IEnumerable<Invoice>> GetAll() => Ok(Invoices);

    [HttpPost]
    [Authorize]  // ?? Protected - only authenticated users
    public ActionResult<Invoice> Create(Invoice invoice) 
    {
      // Only authenticated users can create invoices
    }

    [HttpPut("{id:int}")]
    [Authorize]  // ?? Protected - only authenticated users
    public IActionResult Update(int id, Invoice updated)
    {
        // Only authenticated users can update invoices
    }

 [HttpDelete("{id:int}")]
    [Authorize]  // ?? Protected - only authenticated users
    public IActionResult Delete(int id)
    {
     // Only authenticated users can delete invoices
    }
}
```

## Accessing User Information

Once authenticated, you can access the current user's information from the JWT claims:

```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoiceController : ControllerBase
{
    [HttpPost]
    public ActionResult<Invoice> Create(Invoice invoice)
 {
        // Get user ID from JWT claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userId = int.Parse(userIdClaim ?? "0");

        // Get user email from JWT claims
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

        // Get user name from JWT claims
  var userName = User.FindFirst(ClaimTypes.Name)?.Value;

        // Use the user information
        invoice.CreatedBy = userId;
        invoice.CreatedByEmail = userEmail;
        
        // ... rest of logic
    }
}
```

## Testing Protected Endpoints

### 1. Try Without Authentication (Should Fail)

```bash
curl -X GET http://localhost:5000/api/invoice
```

Expected Response: **401 Unauthorized**

### 2. Login to Get Token

```bash
TOKEN=$(curl -s -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password": "SecurePassword123"
  }' | jq -r '.token')
```

### 3. Access Protected Endpoint with Token

```bash
curl -X GET http://localhost:5000/api/invoice \
  -H "Authorization: Bearer $TOKEN"
```

Expected Response: **200 OK** with data

## PowerShell Example

```powershell
# Login
$loginBody = @{
    email = "john@example.com"
    password = "SecurePassword123"
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri "http://localhost:5000/auth/login" `
    -Method Post `
    -ContentType "application/json" `
    -Body $loginBody

$token = $loginResponse.token

# Access protected endpoint
$headers = @{
    "Authorization" = "Bearer $token"
}

$invoices = Invoke-RestMethod -Uri "http://localhost:5000/api/invoice" `
    -Method Get `
    -Headers $headers

Write-Host "Invoices: $($invoices | ConvertTo-Json)"
```

## Role-Based Authorization (Future Enhancement)

You can also implement role-based authorization:

```csharp
[Authorize(Roles = "Admin")]
[HttpDelete("{id:int}")]
public IActionResult Delete(int id)
{
    // Only users with Admin role can delete
}

[Authorize(Roles = "Admin,Manager")]
[HttpPut("{id:int}")]
public IActionResult Update(int id, Invoice updated)
{
    // Users with Admin or Manager role can update
}

[Authorize(Policy = "RequireAdminRole")]
[HttpPost("bulk-import")]
public IActionResult BulkImport(List<Invoice> invoices)
{
    // Custom policy-based authorization
}
```

### Adding Roles to JWT

Modify `JwtService.cs` to include roles:

```csharp
var claims = new[]
{
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Email, user.Email),
    new Claim(ClaimTypes.Name, user.Name),
    new Claim(ClaimTypes.Role, "User"),  // Add role claim
    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
};
```

## Custom Authorization Policies

Define custom policies in `Program.cs`:

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("RequireEmailVerified", policy =>
     policy.RequireClaim("email_verified", "true"));

    options.AddPolicy("MinimumAge", policy =>
        policy.Requirements.Add(new MinimumAgeRequirement(18)));
});
```

Use in controllers:

```csharp
[Authorize(Policy = "RequireAdminRole")]
[HttpDelete("{id:int}")]
public IActionResult Delete(int id)
{
    // Only admins can delete
}
```

## Common HTTP Status Codes

| Status Code | Description |
|-------------|-------------|
| 200 OK | Request successful with valid token |
| 401 Unauthorized | No token, invalid token, or expired token |
| 403 Forbidden | Valid token but insufficient permissions |

## Error Responses

### Missing Token
Request:
```bash
curl -X GET http://localhost:5000/api/invoice
```

Response: **401 Unauthorized**

### Expired Token
Request:
```bash
curl -X GET http://localhost:5000/api/invoice \
  -H "Authorization: Bearer <expired_token>"
```

Response: **401 Unauthorized**

### Invalid Token
Request:
```bash
curl -X GET http://localhost:5000/api/invoice \
  -H "Authorization: Bearer invalid.token.here"
```

Response: **401 Unauthorized**

## Best Practices

1. **Use HTTPS:** Always use HTTPS in production to prevent token interception
2. **Short Token Expiry:** Use short expiry times (15-30 minutes) for better security
3. **Implement Refresh Tokens:** For better UX with short-lived access tokens
4. **Validate on Every Request:** JWT validation happens automatically
5. **Don't Store Tokens in LocalStorage:** Use httpOnly cookies or secure storage
6. **Log Authentication Failures:** Monitor for suspicious activity

## AllowAnonymous Attribute

To make a specific endpoint public within a protected controller:

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]  // All endpoints protected by default
public class InvoiceController : ControllerBase
{
    [AllowAnonymous]  // ? This endpoint is public
    [HttpGet("public")]
    public ActionResult<string> GetPublicInfo()
    {
        return Ok("This is public information");
 }

    [HttpGet]  // ?? This endpoint requires authentication
    public ActionResult<IEnumerable<Invoice>> GetAll()
    {
     return Ok(Invoices);
    }
}
```

## Complete Example

Here's a complete example with mixed authentication requirements:

```csharp
using InvoiceApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InvoiceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoiceController : ControllerBase
{
    private static readonly List<Invoice> Invoices = new();
    private static int _nextId = 1;
    private readonly ILogger<InvoiceController> _logger;

    public InvoiceController(ILogger<InvoiceController> logger)
    {
        _logger = logger;
    }

  // ? Public - anyone can view invoices
    [HttpGet]
    [AllowAnonymous]
    public ActionResult<IEnumerable<Invoice>> GetAll() 
    {
    _logger.LogInformation("Public access to invoice list");
        return Ok(Invoices);
    }

    // ?? Protected - only authenticated users
    [HttpPost]
    [Authorize]
    public ActionResult<Invoice> Create(Invoice invoice)
    {
     var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

        invoice.Id = _nextId++;
        Invoices.Add(invoice);

 _logger.LogInformation(
            "Invoice {InvoiceId} created by user {UserId} ({Email})",
 invoice.Id, userId, userEmail);

        return CreatedAtAction(nameof(Get), new { id = invoice.Id }, invoice);
    }

    // ?? Protected - only authenticated users
    [HttpPut("{id:int}")]
    [Authorize]
  public IActionResult Update(int id, Invoice updated)
    {
        var invoice = Invoices.FirstOrDefault(i => i.Id == id);
        if (invoice is null) return NotFound();

 var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("Invoice {InvoiceId} updated by user {UserId}", id, userId);

    invoice.Customer = updated.Customer;
     invoice.Date = updated.Date;
    invoice.Amount = updated.Amount;
        invoice.Status = updated.Status;

        return NoContent();
    }

    // ?? Protected - only authenticated users
    [HttpDelete("{id:int}")]
    [Authorize]
    public IActionResult Delete(int id)
    {
   var invoice = Invoices.FirstOrDefault(i => i.Id == id);
        if (invoice is null) return NotFound();

     var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("Invoice {InvoiceId} deleted by user {UserId}", id, userId);

        Invoices.Remove(invoice);
        return NoContent();
    }

    // ? Public - anyone can view a single invoice
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public ActionResult<Invoice> Get(int id)
    {
        var invoice = Invoices.FirstOrDefault(i => i.Id == id);
        return invoice is null ? NotFound() : Ok(invoice);
    }
}
```

## Summary

- Add `[Authorize]` to protect endpoints
- Use `[AllowAnonymous]` to make specific endpoints public
- Access user information via `User.Claims`
- Test with valid JWT tokens
- Implement role-based or policy-based authorization as needed
- Always use HTTPS in production
