# mock-e-commerce-site

A mock e-commerce site used for educational purposes, with a React frontend and ASP.NET Core backend.

## Running locally

Both servers must be running at the same time. Open two terminals:

**Terminal 1 — Backend** (must start first)
```bash
cd src/backend/MockEcommerce.Api
dotnet run --launch-profile http
# API available at http://localhost:5063
```

**Terminal 2 — Frontend**
```bash
cd src/frontend
npm install   # first time only
npm run dev
# App available at http://localhost:5173
```

The Vite dev server proxies all `/api` requests to `http://localhost:5063`, so the frontend will show "Failed to fetch products" if the backend is not running.

## Running tests

```bash
# Frontend tests (from repo root)
npm test

# Backend tests (from test/backend/MockEcommerce.Api.Tests/)
dotnet test
```

See `SPEC.md` for the full feature specification and `copilot-instructions.md` for architecture details.

