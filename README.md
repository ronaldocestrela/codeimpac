# CodeImpact

## Overview
CodeImpact is a SaaS platform for GitHub contribution analysis and AI-generated professional reports.

### Tech Stack
- Backend: .NET 10 Web API, Entity Framework Core, SQL Server, Clean Architecture
- Frontend: React, Vite, TypeScript, Axios, React Query, TailwindCSS
- Testing: xUnit for backend, Vitest + Testing Library for frontend

## Project Structure
- `CodeImpact.slnx` — .NET solution
- `src/backend/CodeImpact.WebApi/` — ASP.NET Core API project
- `src/backend/CodeImpact.Application/` — application layer and DTOs
- `src/backend/CodeImpact.Domain/` — domain entities and common types
- `src/backend/CodeImpact.Infrastructure/` — persistence, Identity, services, settings
- `src/frontend/` — Vite React frontend scaffold
- `CodeImpact.Tests/` — backend unit test project

## Current Status
- Phase 1: Foundation is implemented for backend authentication and persistence.
- Phase 2: Frontend auth setup is implemented, including login/register pages, token storage, and protected routing.
- Phase 3: GitHub integration is implemented end-to-end, including OAuth link, repository listing, repository selection persistence, and manual pull request sync trigger.
- Phase 4: Contribution list/detail APIs and evidence mapping are available.
- Phase 5: AI summary orchestration is implemented with deterministic prompt building and evidence traceability.
- Phase 6: Executive report generation and persistence are implemented end-to-end (API + frontend).

## What is implemented
### Backend
- JWT-based authentication
- `AuthController` with:
  - `POST /api/auth/register`
  - `POST /api/auth/login`
  - `GET /api/auth/me`
  - `POST /api/auth/refresh`
- `TokenService` for access token and refresh token generation
- `RefreshToken` entity with expiration and revocation support
- GitHub integration scaffolding:
  - `GitHubController` with authorization URL, OAuth callback, repository listing and sync trigger
  - Domain/repository support for `GitHubAccount`
  - Domain/repository support for persisted repository selection per user
  - Application/MediatR handlers for linking GitHub accounts, fetching repositories, saving selected repositories, and triggering PR sync fetch
- Reporting:
  - `Report` entity persisted via EF Core migration
  - Report generation endpoint `POST /api/github/reports`
  - Report history endpoint `GET /api/github/reports`
  - Report detail endpoint `GET /api/github/reports/{reportId}`
  - Executive report schema with KPI metrics, summary, highlights, risks/recommendations, and evidence IDs
- CORS enabled for `http://localhost:5173` for frontend development
- Clean Architecture layering: Domain, Application, Infrastructure, WebApi

### Frontend
- Vite + React + TypeScript scaffold
- TailwindCSS setup with `tailwind.config.cjs` and `postcss.config.cjs`
- React Router routes for `/`, `/login`, `/register`, and `/dashboard`
- React Query provider configured in `main.tsx`
- Central Axios instance in `src/frontend/src/services/api.ts`
- Axios auth interceptor with refresh token retry handling
- Zustand auth store with token persistence
- Login and register pages with backend integration
- Protected route guard for dashboard access
- Frontend unit tests using Vitest

### Testing
- Backend unit tests for token issuance and refresh token behavior
- Frontend unit tests for core app rendering and API config

## What still needs to be implemented
### Frontend
- Complete user profile management and dashboard features
- Data fetching for GitHub contributions and reports
- UI for repository selection and contribution details
- Reports page for generating and viewing executive reports
- More comprehensive frontend integration tests and e2e coverage

### Backend / Future Phases
- GitHub OAuth integration (started)
- Repository sync and PR fetching (started, manual sync placeholder implemented)
- Contribution storage and listing
- AI integration via `IAIOrchestrator` and prompt generation
- Hangfire background jobs for long-running processing
- Export features (Markdown, PDF, DOCX)
- Production hardening, Docker, CI/CD, logging and monitoring

## Getting Started
### Backend
1. Ensure .NET 10 SDK is installed
2. Configure SQL Server and update `src/backend/CodeImpact.WebApi/appsettings.json` with a valid connection string
3. Run the API:

```bash
cd /home/ronaldo/Ronaldo/codeimpact
dotnet run --project src/backend/CodeImpact.WebApi/CodeImpact.WebApi.csproj
```

### Frontend
1. Install Node.js (Node 18+ recommended)
2. Install dependencies:

```bash
cd /home/ronaldo/Ronaldo/codeimpact/src/frontend
npm install
```

3. Run frontend:

```bash
npm run dev
```

### Running tests
Backend:

```bash
cd /home/ronaldo/Ronaldo/codeimpact
dotnet test CodeImpact.Tests/CodeImpact.Tests.csproj
```

Frontend:

```bash
cd /home/ronaldo/Ronaldo/codeimpact/src/frontend
npm test
```

## Notes
- All API calls should be routed through `src/frontend/src/services/api.ts` per project conventions.
- JWT should be stored in memory with `localStorage` fallback for the frontend auth flow.
- Business logic should remain outside controllers and use DTOs for API communication.
- LLM integration must be abstracted and never called directly from controllers.
