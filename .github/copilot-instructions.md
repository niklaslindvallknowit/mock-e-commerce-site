# Copilot Instructions

## Repository Overview

Mock e-commerce site used for educational purposes. It is a monorepo with two independent stacks:

- **Frontend**: React 19 + TypeScript + Vite 6 (`src/frontend/`)
- **Backend**: ASP.NET Core Minimal API on .NET 10 (`src/backend/MockEcommerce.Api/`)

Tests live outside `src/` in a parallel `test/` tree that mirrors the source structure.

## Commands

### Frontend

```bash
# From repo root
npm test                                              # Run all frontend tests (vitest run)
npx vitest run test/frontend/App.test.tsx             # Run a single test file
npx vitest run -t "renders the header"               # Run tests matching a name pattern

# From src/frontend/
npm run dev     # Dev server on http://localhost:5173
npm run build   # tsc + vite build
npm run lint    # ESLint
```

### Backend

```bash
# Run the API (from src/backend/MockEcommerce.Api/)
dotnet run      # Starts on http://localhost:5063; OpenAPI at /openapi/v1.json

# Run tests (from test/backend/MockEcommerce.Api.Tests/)
dotnet test
dotnet test --filter "FullyQualifiedName~ProductEndpointTests"   # Single class
dotnet test --filter "FullyQualifiedName~CartEndpointTests"      # Single class
dotnet test --filter "DisplayName~GetAll"                        # Single test
```

## Architecture

### Frontend (`src/frontend/src/`)

- `api/index.ts` — all HTTP calls (base URL `/api`); exports `fetchProducts`, `fetchProductById`, `addToCart`; also defines a local `CartItem` interface (not re-exported from `types/`)
- `hooks/useProducts.ts` — custom hook wrapping `fetchProducts`; returns `{ products, loading, error }`
- `components/` — presentational components: `Header` (cart icon + count), `HeroBanner`, `ProductList`, `ProductCard` (Add to Cart button disabled when `stock === 0`)
- `types/index.ts` — shared TypeScript interfaces: `Product` and `AddToCartRequest`
- `App.tsx` — top-level orchestration: fetches products via `useProducts`, calls `addToCart` on button click, maintains `cartItemCount` and a 3-second toast notification

**TypeScript interfaces:**

```ts
// src/frontend/src/types/index.ts
interface Product { id, name, description, price, category, stock, imageUrl }
interface AddToCartRequest { productId, quantity }

// src/frontend/src/api/index.ts (local, not exported)
interface CartItem { productId, productName, unitPrice, quantity, totalPrice }
```

### Backend (`src/backend/MockEcommerce.Api/`)

- **Minimal API** — endpoints registered as extension methods in `Endpoints/` and mapped in `Program.cs`
- `Models/Product.cs` — `{ Id, Name, Description, Price, Category, Stock, ImageUrl }`
- `Models/CartItem.cs` — `{ ProductId, ProductName, UnitPrice, Quantity, TotalPrice (computed) }`
- `Endpoints/CartEndpoints.cs` — also defines the `AddToCartRequest` record `(int ProductId, int Quantity)`
- `Services/IProductService` — `GetAll()`, `GetById(int)`
- `Services/MockProductService` — static in-memory list of **5 products** (ids 1–5: "Wireless Headphones" $79.99, "Running Shoes" $59.99, "Stainless Steel Water Bottle" $24.99, "Mechanical Keyboard" $109.99, "Yoga Mat" $34.99), registered as singleton; full catalog in the **Seed Product Data** section below
- `Services/ICartService` — `GetAll()`, `Add(CartItem)`, `GetByProductId(int)`, `Remove(int)`, `Clear()`
- `Services/InMemoryCartService` — thread-safe (`Lock`) list-backed implementation, registered as singleton; **all methods are `NotImplementedException`**
- `Program.cs` — registers services, configures CORS for `http://localhost:5173`, maps endpoints, exposes OpenAPI

### Implementation State

**Already working — do not rewrite:**

- `MockProductService` — fully implemented; `GetAll()` and `GetById(int)` return static data
- `ProductEndpoints` — `GET /api/products` and `GET /api/products/{id}` are fully implemented and tested
- All frontend components, hooks, and API calls — `useProducts`, `fetchProducts`, `fetchProductById`, `addToCart`, `Header`, `HeroBanner`, `ProductList`, `ProductCard`, `App` are complete

**Stubbed — the exercise for learners:**

The shopping cart service and its endpoints both throw `NotImplementedException`. Both layers must be implemented:

1. **`InMemoryCartService`** (`src/backend/MockEcommerce.Api/Services/InMemoryCartService.cs`) — implement `GetAll`, `Add`, `GetByProductId`, `Remove`, `Clear` using the existing `_cart` list and `_lock`
2. **`CartEndpoints` handlers** (`src/backend/MockEcommerce.Api/Endpoints/CartEndpoints.cs`) — implement `GetCart`, `AddToCart`, `RemoveFromCart`, `ClearCart` using `ICartService` (and `IProductService` for `AddToCart`)

**Cart API routes:**

| Method | Route | Handler | Notes |
|--------|-------|---------|-------|
| `GET` | `/api/cart` | `GetCart` | Returns `CartItem[]` |
| `POST` | `/api/cart` | `AddToCart` | Body: `AddToCartRequest`; returns `201 Created` (new) or `200 OK` (incremented); `404` if product not found; `ValidationProblem` if quantity ≤ 0 |
| `DELETE` | `/api/cart/{productId}` | `RemoveFromCart` | `204 NoContent` or `404 NotFound` |
| `DELETE` | `/api/cart` | `ClearCart` | `204 NoContent` |

**Product API routes (already implemented):**

| Method | Route | Notes |
|--------|-------|-------|
| `GET` | `/api/products` | Returns all 5 products |
| `GET` | `/api/products/{id}` | `200 OK` or `404 NotFound` |

### Frontend ↔ Backend

The Vite dev server proxies `/api` to the .NET backend (configured in `src/frontend/vite.config.ts`). CORS is configured on the backend to allow `http://localhost:5173`.

## Seed Product Data

`MockProductService` seeds 5 static products. Tests should use these known IDs and values:

| Id | Name | Category | Price | Stock |
|----|------|----------|-------|-------|
| 1 | Wireless Headphones | Electronics | $79.99 | 25 |
| 2 | Running Shoes | Footwear | $59.99 | 40 |
| 3 | Stainless Steel Water Bottle | Accessories | $24.99 | 100 |
| 4 | Mechanical Keyboard | Electronics | $109.99 | 15 |
| 5 | Yoga Mat | Sports | $34.99 | 60 |

Images use `https://placehold.co/300x300?text=<Name>`.

## Test Conventions

### Frontend (Vitest 4 + Testing Library)

Test files and their source mirrors:

| Test file | Source |
|-----------|--------|
| `test/frontend/App.test.tsx` | `src/frontend/src/App.tsx` |
| `test/frontend/components/Header/Header.test.tsx` | `src/frontend/src/components/Header/Header.tsx` |
| `test/frontend/components/HeroBanner/HeroBanner.test.tsx` | `src/frontend/src/components/HeroBanner/HeroBanner.tsx` |
| `test/frontend/components/ProductCard/ProductCard.test.tsx` | `src/frontend/src/components/ProductCard/ProductCard.tsx` |
| `test/frontend/components/ProductList/ProductList.test.tsx` | `src/frontend/src/components/ProductList/ProductList.tsx` |
| `test/frontend/hooks/useProducts.test.ts` | `src/frontend/src/hooks/useProducts.ts` |

- `vitest.config.ts` at repo root configures the test runner (not the one inside `src/frontend/`)
- Mock hooks and the `api` module with `vi.mock(...)` at the top of the test file, then use `vi.mocked()` to type the mock
- `src/frontend/src/test-setup.ts` sets up `@testing-library/jest-dom` matchers

### Backend (xUnit)

Test files:

| Test file | Type | What it covers |
|-----------|------|----------------|
| `test/backend/MockEcommerce.Api.Tests/Services/MockProductServiceTests.cs` | Unit | `MockProductService` |
| `test/backend/MockEcommerce.Api.Tests/Endpoints/ProductEndpointTests.cs` | Integration | `GET /api/products` endpoints |

- Unit tests instantiate services directly
- Integration tests use `WebApplicationFactory<Program>` (the `Program` partial class in `Program.cs` enables this)
- Test method naming: `MethodName_Condition_ExpectedBehavior`
- Cart tests do not yet exist — they are part of the exercise
