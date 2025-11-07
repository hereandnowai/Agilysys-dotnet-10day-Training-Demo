# First Sample Project - .NET 8 Web API

A clean, production-ready .NET 8 REST API with structured logging, request tracking, and containerization support.

## ?? Table of Contents

- [Features](#features)
- [Prerequisites](#prerequisites)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Development](#development)
- [Docker](#docker)
- [Logging](#logging)
- [API Documentation](#api-documentation)

## ? Features

- ? .NET 8 Web API
- ? Structured logging with Serilog
- ? Request/Response logging middleware
- ? Swagger/OpenAPI documentation
- ? Docker support
- ? Clean architecture ready
- ? JSON-formatted logs with file rotation

## ?? Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker](https://www.docker.com/get-started) (optional, for containerization)
- Visual Studio 2022 or VS Code (recommended)

## ?? Project Structure

```
First_Sample_Project_Prompting/
??? First_Sample_Project_Prompting/    # Main API project
?   ??? Controllers/     # API controllers
?   ??? Models/   # Data models and DTOs
?   ??? Middleware/     # Custom middleware components
?   ??? Services/  # Business logic services
?   ??? Program.cs           # Application entry point
?   ??? appsettings.json               # Configuration
??? config/           # Configuration files
??? logs/              # Application logs (auto-generated)
??? docs/        # Documentation
??? Dockerfile   # Docker configuration
??? .gitignore# Git ignore rules
??? README.md      # This file
```

## ?? Getting Started

### Local Development Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd First_Sample_Project_Prompting
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Build the project**
   ```bash
   dotnet build
   ```

4. **Run the application**
   ```bash
   dotnet run --project First_Sample_Project_Prompting
   ```

5. **Access the API**
   - API: `https://localhost:5001` or `http://localhost:5000`
   - Swagger UI: `https://localhost:5001/swagger`

## ?? Development

### Running in Development Mode

```bash
cd First_Sample_Project_Prompting
dotnet watch run
```

This enables hot reload - the app will automatically restart when code changes are detected.

### Running Tests

```bash
dotnet test
```

### Configuration

Edit `First_Sample_Project_Prompting/appsettings.json` or `config/appsettings.Production.json` for environment-specific settings.

## ?? Docker

### Build Docker Image

```bash
docker build -t first-sample-api:latest .
```

### Run Docker Container

```bash
docker run -d -p 8080:8080 --name first-sample-api first-sample-api:latest
```

Access the API at `http://localhost:8080`

### Docker Compose (Optional)

If you have a `docker-compose.yml`:

```bash
docker-compose up -d
```

## ?? Logging

The application uses **Serilog** for structured logging with the following features:

- **Console Output**: Color-coded logs in development
- **File Output**: JSON-formatted logs in `logs/app.log`
- **Log Rotation**: Daily rotation with 31-day retention
- **Request Logging**: Automatic logging of all HTTP requests

### Log Fields

Each HTTP request log includes:
- `Timestamp`: Request time (ISO 8601)
- `Level`: Log level (Information, Warning, Error)
- `Method`: HTTP method (GET, POST, etc.)
- `Path`: Request path
- `StatusCode`: HTTP response status
- `Latency`: Request duration in milliseconds
- `Message`: Descriptive message

### Log Location

- Development: Console + `logs/app.log`
- Production: `logs/app.log` (structured JSON)

## ?? API Documentation

### Swagger/OpenAPI

Access interactive API documentation at:
- **Development**: `https://localhost:5001/swagger`
- **Production**: `https://<your-domain>/swagger` (if enabled)

### Available Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/weatherforecast` | Get weather forecast data |

*(Add more endpoints as you build the API)*

## ??? Development Guidelines

This project follows strict coding standards defined in `.github/Rules/copilot-instructions.md`:

- **Architecture**: Clean, modular design (Controller ? Service ? Repository)
- **Dependency Injection**: All services registered in DI container
- **Error Handling**: Global exception middleware
- **Naming Conventions**: PascalCase for public members, camelCase with `_` prefix for private fields
- **Documentation**: XML comments on all public methods
- **Testing**: xUnit with Moq, 80% minimum coverage
- **SOLID Principles**: Applied throughout the codebase

## ?? Contributing

1. Follow the coding standards in `copilot-instructions.md`
2. Write unit tests for new features
3. Update documentation as needed
4. Keep methods under 30 lines
5. Add XML documentation comments

## ?? License

[Your License Here]

## ?? Authors

[Your Name/Team]

---

**Happy Coding! ??**
