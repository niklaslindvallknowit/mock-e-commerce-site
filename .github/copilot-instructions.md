# Copilot Instructions

## Repository Overview

Mock e-commerce site used for educational purposes. It is a monorepo with two independent stacks:

- **Frontend**: React 19 + TypeScript + Vite (`src/frontend/`)
- **Backend**: ASP.NET Core Minimal API on .NET 10 (`src/backend/MockEcommerce.Api/`)

Tests live outside `src/` in a parallel `test/` tree that mirrors the source structure.

## Commands

### Frontend

```bash
# From repo root
npm test                          # Run all frontend tests (vitest run)
npx vitest run test/frontend/App.test.tsx  # Run a single test file
npx vitest run -t "renders the header"     # Run tests matching a name pattern

# From src/frontend/
npm run dev     # Dev server on http://localhost:5173
npm run build   # tsc + vite build
npm run lint    # ESLint
```

### Backend

```bash
# From test/backend/MockEcommerce.Api.Tests/
dotnet test
dotnet test --filter "FullyQualifiedName~ProductEndpointTests"  # Single class
dotnet test --filter "DisplayName~GetAll"                       # Single test
```

## Architecture

### Frontend (`src/frontend/src/`)

- `api/index.ts` — all HTTP calls, base URL is `/api`
- `hooks/` — custom hooks wrapping the API (e.g. `useProducts` returns `{ products, loading, error }`)
- `components/` — presentational components (`Header`, `HeroBanner`, `ProductList`, `ProductCard`)
- `types/index.ts` — shared TypeScript interfaces (`Product`, `AddToCartRequest`)
- `App.tsx` — top-level orchestration: fetches products via `useProducts`, handles cart interactions

### Backend (`src/backend/MockEcommerce.Api/`)

- **Minimal API** — endpoints are registered as extension methods in `Endpoints/` and mapped in `Program.cs`
- `Services/` — `MockProductService` (static in-memory data), `InMemoryCartService`; both injected as singletons via interfaces
- Cart endpoints in `CartEndpoints.cs` are **intentionally `NotImplementedException`** — this is the exercise for learners

### Frontend ↔ Backend

The Vite dev server proxies `/api` to the .NET backend (configured in `src/frontend/vite.config.ts`). CORS is configured on the backend to allow `http://localhost:5173`.

## Test Conventions

### Frontend (Vitest + Testing Library)

- Test files live in `test/frontend/`, mirroring `src/frontend/src/`
- `vitest.config.ts` at repo root configures the test runner (not the one in `src/frontend/`)
- Mock hooks and the `api` module with `vi.mock(...)` at the top of the test file, then use `vi.mocked()` to type the mock
- `src/frontend/src/test-setup.ts` sets up `@testing-library/jest-dom` matchers

### Backend (xUnit)

- Unit tests instantiate services directly; integration tests use `WebApplicationFactory<Program>` (the `Program` partial class in `Program.cs` enables this)
- Test method naming: `MethodName_Condition_ExpectedBehavior`
