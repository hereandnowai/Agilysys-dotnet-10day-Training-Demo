# Epic 1 Prompts — Project Setup & Scaffolding

## Story 1.1 — Create Repo & Project Skeleton

### Prompt

```
Create a minimal .NET 8 REST API project skeleton with the following structure:
- src/ folder containing the API project
- config/ folder for configuration files
- logs/ folder for log files
- docs/ folder for documentation
- Add a controllers folder, models folder, and routes setup
- Include a comprehensive README.md with instructions for running locally and development setup
- Add a Dockerfile for containerization
- Include a .gitignore file appropriate for .NET projects

The API should be a clean, minimal setup ready for development.
```

### Acceptance Criteria
- ✅ Repo created with README containing run and dev instructions
- ✅ Basic folder layout: src/, config/, logs/, docs/
- ✅ .gitignore added

### Labels
`infra`, `setup`, `copilot`

### Estimate
1 SP

---

## Story 1.2 — Add Logging & Request Logging Middleware

### Prompt

```
Add structured logging and request logging middleware to the .NET 8 Web API using Serilog with the following requirements:
- Log timestamp, log level, message, request path, HTTP method, response status code, and request latency
- Configure logging to output to both console and a file at logs/app.log
- Implement middleware that logs all incoming HTTP requests and their responses
- Use structured logging format (JSON preferred)
- Configure log file rotation or append mode
- Include request/response logging middleware in the pipeline

Ensure logs are readable and include all required fields for debugging and monitoring.
```

### Acceptance Criteria
- ✅ Logs show timestamp, level, message, request path, method, status, and latency
- ✅ Log files rotate or append to logs/app.log

### Labels
`infra`, `logging`

### Estimate
2 SP

---

## Combined Prompt (Both Stories)

If you want to implement both stories together, use this combined prompt:

```
Create a .NET 8 Web API project with:

1. Project Structure:
   - Organized folders: src/, config/, logs/, docs/
   - Controllers, models, and routes setup
   - README.md with local run and development instructions
   - Dockerfile for containerization
   - Appropriate .gitignore

2. Logging & Middleware:
   - Serilog integration for structured logging
   - Request logging middleware capturing: timestamp, method, path, status code, and latency
   - Dual output: console and logs/app.log with rotation
   - JSON-formatted structured logs

The project should be production-ready with proper logging infrastructure.
```

---

## Usage Instructions

1. Copy the relevant prompt (Story 1.1, 1.2, or Combined)
2. Paste it into GitHub Copilot Chat or your AI assistant
3. Review the generated code and files
4. Validate against the acceptance criteria
5. Test the implementation locally

## Notes

- Stack: .NET 8 Web API
- Logging Library: Serilog (recommended for .NET)
- All prompts are designed to be self-contained and executable
