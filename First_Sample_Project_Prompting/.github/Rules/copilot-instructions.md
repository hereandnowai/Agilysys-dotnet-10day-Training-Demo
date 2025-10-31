# GitHub Copilot Assistant Instructions
**Purpose:**  
These instructions define the global development conventions for this repository. Copilot should follow these conventions unless explicitly overridden in a feature-specific prompt.

---

## 1. General Coding Standards
- Target Framework: .NET 8
- Language: C# 
- Architecture: Clean and modular (Controller → Service → Repository → Database)
- Use dependency injection for all services.
- Follow SOLID principles.
- Use PascalCase for classes, methods, and properties.
- Use camelCase for private fields and parameters with underscore as prefix.
- Avoid regions and redundant comments.
- For every method include XML documentation comments.
- For every method include unit tests.
- Every method should not exceed 30 lines of code; refactor into smaller methods if necessary.
- Add understandable and meaningful names for variables, methods, and classes.

---

## 2. Exception Handling
- Always handle exceptions gracefully.
- Use `try-catch` blocks in service methods.
- Do not repeat try-catch in controllers if a global exception middleware or filter is present.
- Log all exceptions with contextual details.
- Never expose internal error messages to API consumers; return standardized error responses.

---

## 3. Logging
- Use Microsoft.Extensions.Logging or Serilog.
- Log key actions, warnings, and exceptions.
- Avoid logging sensitive information (passwords, tokens, etc.).
- Use structured logging where possible.

---

## 4. Entity Framework Core
- Use EF Core 8 with code-first approach.
- Keep `DbContext` lightweight and scoped per request.
- Configure relationships and constraints with Fluent API.
- Use `AsNoTracking()` for read-only queries.
- Implement repository or data-access layer to encapsulate EF Core operations.

---

## 5. Controller Design
- Keep controllers thin; delegate all logic to services.
- Use `[ApiController]` and route naming convention: `api/[controller]`.
- Return `ActionResult<T>` or `IActionResult` consistently.
- Validate input models using `[Required]`, `[StringLength]`, etc.
- Return proper HTTP status codes:
  - 200 / 201 for success
  - 400 for bad requests
  - 404 for not found
  - 500 for internal errors

---

## 6. Service Layer
- Encapsulate all business logic in services.
- Use interfaces (e.g., `IOrderService`) and register them with dependency injection.
- Apply `try-catch` blocks within services and log errors.
- Return typed results or domain objects to controllers.
- Keep methods short and focused on a single responsibility.

---

## 7. DTOs and Models
- Use Data Transfer Objects (DTOs) to separate API contracts from EF entities.
- Apply validation attributes on DTOs, not entities.
- Map between DTOs and entities using AutoMapper or manual conversion methods.
- Keep entities clean and focused on persistence.

---

## 8. Testing
- Use xUnit or NUnit for unit testing.
- Mock dependencies using Moq or similar libraries.
- Write one test class per service or controller.
- Use meaningful test names following the pattern:
  `MethodName_Scenario_ExpectedResult`.
- Ensure 80% minimum test coverage on core modules.

---

## 9. API Consistency
- All endpoints should be RESTful.
- Use plural resource names (e.g., `/api/orders`).
- Support standard CRUD operations:
  - GET (list or detail)
  - POST (create)
  - PUT/PATCH (update)
  - DELETE (remove)
- Support pagination and filtering for list endpoints.

---

## 10. Security and Configuration
- Use appsettings.json for configuration values.
- Never hardcode credentials or secrets.
- Use environment variables or secret managers for sensitive data.
- Apply `[Authorize]` attributes where necessary.
- Sanitize and validate all external inputs.

---

## 11. Documentation
- Add XML documentation comments to public classes and methods.
- Include summaries, parameter descriptions, and return values.
- Maintain an up-to-date README for setup and usage instructions.

---

## 12. Copilot Behavior
- Always follow these conventions unless explicitly overridden in a prompt or comment.
- Before generating code, Copilot should clarify any missing details (e.g., entity names, fields, or options).
- Avoid adding unnecessary frameworks, libraries, or boilerplate code not requested.
- Maintain readability and consistency across files.

---

**End of Instructions**
