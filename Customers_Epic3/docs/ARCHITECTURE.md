# Architecture Documentation

## Overview

This project follows a clean, layered architecture pattern to ensure maintainability, testability, and scalability.

## Architecture Layers

```
???????????????????????????????????????
?      Controllers          ?  ? HTTP Endpoints (Thin layer)
???????????????????????????????????????
           ?
???????????????????????????????????????
?          Services       ?  ? Business Logic
???????????????????????????????????????
  ?
???????????????????????????????????????
? Repositories    ?  ? Data Access
???????????????????????????????????????
    ?
???????????????????????????????????????
?         Database   ?  ? Persistence Layer
???????????????????????????????????????
```

## Components

### 1. Controllers
- Thin layer handling HTTP requests/responses
- Input validation using data annotations
- Delegates all logic to services
- Returns `ActionResult<T>` or `IActionResult`

**Example:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    
    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll()
    {
        var orders = await _orderService.GetAllOrdersAsync();
        return Ok(orders);
    }
}
```

### 2. Services
- Contains all business logic
- Registered with dependency injection
- Uses interfaces for abstraction
- Handles exceptions and logging

**Example:**
```csharp
public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
}

public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly ILogger<OrderService> _logger;
    
    public OrderService(IOrderRepository repository, ILogger<OrderService> logger)
    {
        _repository = repository;
        _logger = logger;
  }
    
    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
     try
        {
          var orders = await _repository.GetAllAsync();
            return orders.Select(MapToDto);
    }
        catch (Exception ex)
   {
            _logger.LogError(ex, "Error retrieving orders");
            throw;
        }
    }
}
```

### 3. Repositories
- Encapsulates data access logic
- Uses Entity Framework Core
- Implements repository pattern
- Returns domain entities

### 4. Models
- **Entities**: Domain models (EF Core entities)
- **DTOs**: Data Transfer Objects for API contracts
- **ViewModels**: UI-specific models (if needed)

## Design Principles

### SOLID Principles

1. **Single Responsibility**: Each class has one reason to change
2. **Open/Closed**: Open for extension, closed for modification
3. **Liskov Substitution**: Derived classes can substitute base classes
4. **Interface Segregation**: Clients shouldn't depend on unused interfaces
5. **Dependency Inversion**: Depend on abstractions, not concretions

### Dependency Injection

All services are registered in `Program.cs`:

```csharp
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
```

## Middleware Pipeline

```
HTTP Request
    ?
Exception Handling Middleware
    ?
Request Logging Middleware
    ?
Authentication/Authorization
    ?
Routing
    ?
Controller Action
    ?
Response
```

## Error Handling

- Global exception middleware catches all unhandled exceptions
- Services use try-catch for specific error handling
- Errors are logged with contextual information
- Standardized error responses returned to clients

## Logging Strategy

- **Serilog**: Structured logging framework
- **Request Logging**: All HTTP requests logged with metadata
- **Error Logging**: Exceptions logged with stack traces
- **Performance Logging**: Latency tracking for requests

## Future Enhancements

- [ ] Add CQRS pattern for complex domains
- [ ] Implement MediatR for request/response handling
- [ ] Add FluentValidation for complex validation rules
- [ ] Implement specification pattern for queries
- [ ] Add domain events for decoupling
