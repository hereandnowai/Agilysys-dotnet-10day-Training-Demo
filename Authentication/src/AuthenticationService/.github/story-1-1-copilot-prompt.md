# Story 1.1 — Create repo & project skeleton

## Story Details

**Epic:** Project setup & scaffolding  
**Story ID:** 1.1  
**Estimate:** 1 SP  
**Labels:** infra, setup, copilot

## Description
Initialize Git repo and create project skeleton (stack: .NET Web API). Add README with run instructions,inside InvoiceApi soiultion .

## Acceptance Criteria
- ✅ Repo created with README containing run and dev instructions
- ✅ Basic folder layout: src/, config/, logs/, docs/
- ✅ .gitignore added

## Copilot Prompt

```
Create a minimal .NET Web API REST API project skeleton with routes folder, controllers, models folder, config, and a README describing how to run locally. Use Dockerfile and .gitignore.
```

## Implementation Steps

1. **Copy the copilot prompt** above
2. **Paste into GitHub Copilot Chat** or your IDE
3. **Follow the generated suggestions** to create:
   - Project structure with folders
   - Basic controllers and models
   - Configuration setup
   - README with run instructions
   - Dockerfile for containerization
   - .gitignore file
4. **Verify implementation** against acceptance criteria
5. **Test locally** to ensure everything works

## Expected Deliverables

- [ ] Git repository initialized
- [ ] Project folder structure created
- [ ] Basic .NET Web API project files
- [ ] README.md with clear instructions
- [ ] Dockerfile for containerization
- [ ] .gitignore file configured for .NET projects
- [ ] Basic controllers and models structure
- [ ] Configuration folder setup

## Notes
- Focus on creating a clean, minimal skeleton
- Ensure the project can be run locally following README instructions
- Structure should support microservice architecture patterns
- Use standard .NET Web API conventions