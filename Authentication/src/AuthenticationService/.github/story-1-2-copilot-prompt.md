# Story 1.2 — Add logging & request logging middleware

## Story Details

**Epic:** Project setup & scaffolding  
**Story ID:** 1.2  
**Estimate:** 2 SP  
**Labels:** infra, logging

## Description
Integrate structured logging (e.g., Winston/Loguru/Serilog) and a middleware to log incoming requests and responses (status). Logs written to console and file.

## Acceptance Criteria
- ✅ Logs show timestamp, level, message, request path, method, status, and latency
- ✅ Log files rotate or append to logs/app.log

## Copilot Prompt

```
Add request logging middleware using Serilog that logs timestamp, method, path, status, latency, and writes to console and logs/app.log.
```

## Implementation Steps

1. **Copy the copilot prompt** above
2. **Paste into GitHub Copilot Chat** or your IDE
3. **Follow the generated suggestions** to implement:
   - Serilog configuration and setup
   - Request logging middleware
   - Console and file logging configuration
   - Log formatting with required fields
   - Log file management (rotation/append)
4. **Configure logging** in Program.cs or Startup.cs
5. **Test the middleware** with sample requests
6. **Verify logs** are written to both console and file

## Expected Deliverables

- [ ] Serilog NuGet packages installed
- [ ] Serilog configuration setup
- [ ] Custom request logging middleware implemented
- [ ] Middleware registered in the pipeline
- [ ] Console logging configured
- [ ] File logging to logs/app.log configured
- [ ] Log format includes all required fields:
  - [ ] Timestamp
  - [ ] Log level
  - [ ] Request method
  - [ ] Request path
  - [ ] Response status code
  - [ ] Request latency/duration
- [ ] Log file rotation or append functionality
- [ ] Testing with sample API calls

## Technical Requirements

### Log Format Example
```
2025-10-30 10:15:30.123 [INF] HTTP GET /api/users responded 200 in 45ms
```

### Required Log Fields
- **Timestamp**: ISO 8601 format
- **Level**: INFO, WARN, ERROR, etc.
- **Method**: HTTP method (GET, POST, etc.)
- **Path**: Request path/endpoint
- **Status**: HTTP response status code
- **Latency**: Request processing time in milliseconds

### Configuration
- Logs should write to both console and file simultaneously
- File location: `logs/app.log`
- Consider log file size limits and rotation strategy

## Dependencies
- Serilog
- Serilog.AspNetCore
- Serilog.Sinks.Console
- Serilog.Sinks.File

## Notes
- This story builds upon Story 1.1 (project skeleton)
- Ensure middleware is properly positioned in the request pipeline
- Consider performance impact of logging on high-traffic scenarios
- Use structured logging for better log analysis capabilities