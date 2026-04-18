# IMPLEMENTATION_ROADMAP.md
## Logical Order for Building the System

# Phase 1 — Foundation
1. Create solution structure
2. Setup .NET API
3. Setup SQL Server + EF Core
4. Configure Identity + JWT
5. Create BaseEntity
6. Create initial entities

# Phase 2 — Frontend Setup
1. Create Vite + React + TypeScript app
2. Setup routing
3. Setup API service (Axios)
4. Setup React Query
5. Setup auth flow

*Status: Phase 2 implemented — login/register, auth state, refresh handling, and protected routes are available.*

# Phase 3 — GitHub Integration
1. Implement GitHub OAuth (implemented)
2. Fetch repositories (implemented)
3. Select repositories (implemented with persistence)
4. Sync Pull Requests (implemented as real GitHub fetch trigger)
5. Sync commits for selected repositories (pending)
6. Sync PR reviews and approval events (pending)

*Status: OAuth + repository selection are implemented. Commit sync and PR review/approval ingestion are required to complete the domain premise.*

# Phase 4 — Contributions
1. Store commits
2. Store PRs and PR reviews
3. Define and persist approved PR classification rules (`APPROVED` review state)
4. List contributions (commits + PRs)
5. Detail contribution with evidence links
6. Basic classification (no AI)

# Phase 5 — AI Integration
1. Create ILLMService
2. Implement OpenAIService
3. Create PromptBuilder
4. Create AIOrchestrator
5. Generate summaries from commits and approved PRs
6. Generate manager-oriented highlights with evidence references

# Phase 6 — Reports
1. Create Report entity
2. Define executive report schema (KPIs + narrative + evidence)
3. Aggregate contributions
4. Store results
5. Generate report for leadership presentation

# Phase 7 — Frontend Features
1. Dashboard
2. Contributions page
3. Report page
4. Repository and period filters for report scope
5. Export UI

# Phase 8 — Background Jobs
1. Setup Hangfire
2. Move AI processing to jobs
3. Move report generation to jobs

# Phase 9 — Export
1. Markdown export
2. PDF export
3. DOCX export

# Phase 10 — Production
1. Docker setup
2. CI/CD
3. Logging
4. Monitoring
