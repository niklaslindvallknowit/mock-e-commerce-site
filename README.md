# mock-e-commerce-site

A mock e-commerce site used for educational purposes, with a React frontend and ASP.NET Core backend.

## Running locally

From the **repo root**, run both servers with one command:

```bash
npm install   # first time only
npm run dev
```

This starts:
- **Backend** on `http://localhost:5063`
- **Frontend** on `http://localhost:5173` ← open this in your browser

The Vite dev server proxies all `/api` requests to the backend. If you prefer to start them separately, see the two-terminal instructions below.

<details>
<summary>Start servers separately</summary>

**Terminal 1 — Backend**
```bash
cd src/backend/MockEcommerce.Api
dotnet run --launch-profile http
```

**Terminal 2 — Frontend**
```bash
cd src/frontend
npm run dev
```
</details>

## Running tests

```bash
# Frontend tests (from repo root)
npm test

# Backend tests (from test/backend/MockEcommerce.Api.Tests/)
dotnet test
```

See `SPEC.md` for the full feature specification and `copilot-instructions.md` for architecture details.

