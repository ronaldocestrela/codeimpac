# AGENTS.md
## Full Instructions for AI Agent Implementation

# Project Overview
This project is a SaaS platform that:
- Connects to GitHub
- Analyzes developer contributions
- Uses LLM to generate professional reports

# Tech Stack

## Backend
- .NET 10 Web API
- SQL Server
- Entity Framework Core
- Clean Architecture
- Hangfire (background jobs)

## Frontend
- React (Vite + TypeScript)
- Client-side rendering
- Axios for API
- React Query for data fetching
- TailwindCSS for UI

---

# Frontend Architecture

## Structure
```
src/
 ├── app/
 ├── components/
 ├── pages/
 ├── hooks/
 ├── services/
 ├── store/
 ├── types/
 ├── utils/
 └── routes/
```

## Core Concepts

### State Management
- Use React Query for server state
- Use Zustand or Context API for client state

### API Layer
All API calls must go through:
```
services/api.ts
```

### Authentication
- JWT stored in memory + localStorage fallback
- Interceptor for token injection

---

# Backend Rules

- Follow Clean Architecture strictly
- No business logic in controllers
- Use MediatR pattern (commands/queries)
- LLM must be abstracted via interface

---

# AI Integration Rules

- Never call LLM directly in controller
- Use IAIOrchestrator
- Always log prompt and response
- Use retry policies

---

# Coding Standards

- Use SOLID principles
- Use async/await everywhere
- Always validate input
- Use DTOs for API communication
- Never expose domain entities directly

---

# Testing

- Unit tests for Application layer
- Integration tests for API
- Mock external services (GitHub, LLM)

---

# Definition of Done

A feature is complete when:
- API endpoint works
- Frontend integrated
- LLM output validated
- Logs are stored
- Tests pass
