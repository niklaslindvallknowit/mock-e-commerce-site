# Cart Feature Specification

## Overview

Users can add products to a shared in-memory cart, view the cart in a side drawer accessible from the header icon, adjust item quantities, remove individual items, and see a running total before checkout.

---

## Backend

### Implement existing stubs

All four handlers in `CartEndpoints.cs` and all five methods in `InMemoryCartService.cs` must be implemented (currently all throw `NotImplementedException`).

#### `InMemoryCartService` — method contracts

| Method | Behaviour |
|--------|-----------|
| `GetAll()` | Returns a snapshot of all items in `_cart` under `_lock` |
| `GetByProductId(int productId)` | Returns the matching `CartItem` or `null` |
| `Add(CartItem item)` | If the product is already in the cart, increments `Quantity`; otherwise appends the item. Returns the current state of the item. Must be executed under `_lock`. |
| `Remove(int productId)` | Removes the item; returns `true` if found, `false` otherwise. Under `_lock`. |
| `Clear()` | Empties `_cart`. Under `_lock`. |

#### `CartEndpoints` — handler contracts

| Handler | Method + Route | Success | Error cases |
|---------|----------------|---------|-------------|
| `GetCart` | `GET /api/cart` | `200 OK` → `CartItem[]` | — |
| `AddToCart` | `POST /api/cart` | `201 Created` (new item) or `200 OK` (incremented) → `CartItem` | `404` product not found; `400 ValidationProblem` if `Quantity ≤ 0` or resulting quantity would exceed 5 |
| `RemoveFromCart` | `DELETE /api/cart/{productId}` | `204 NoContent` | `404 NotFound` if not in cart |
| `ClearCart` | `DELETE /api/cart` | `204 NoContent` | — |

### New endpoint: PUT /api/cart/{productId}

Sets the quantity of an item already in the cart to an exact value.

**Route:** `PUT /api/cart/{productId:int}`  
**Request body:** `{ "quantity": <int> }`  — model: `UpdateCartItemRequest(int Quantity)`

| Condition | Response |
|-----------|----------|
| Product not in cart | `404 NotFound` |
| `Quantity < 0` | `400 ValidationProblem` — "Quantity must be 0 or greater" |
| `Quantity == 0` | Remove item → `204 NoContent` |
| `Quantity > 5` | `400 ValidationProblem` — "Quantity cannot exceed 5" |
| `Quantity` 1–5 | Update item → `200 OK` → `CartItem` |

**Handler signature:**
```csharp
internal static Results<Ok<CartItem>, NoContent, NotFound, ValidationProblem> UpdateCartItem(
    int productId, UpdateCartItemRequest request, ICartService cartService)
```

**`ICartService` addition required:**
```csharp
/// <summary>Sets the quantity of an existing cart item. Caller must verify the item exists first.</summary>
CartItem Update(int productId, int quantity);
```

### Validation rules summary

- `POST /api/cart` — `Quantity` must be ≥ 1; `existing quantity + requested quantity` must be ≤ 5
- `PUT /api/cart/{productId}` — `Quantity` must be ≥ 0; if 1–5 update; if 0 remove; if > 5 reject
- Both endpoints: `productId` must refer to an existing product (`MockProductService.GetById`)

### Shared cart note

`InMemoryCartService` is a singleton — all users share the same cart. This is intentional for the demo; no authentication is in scope.

---

## Frontend

### New API functions (`src/frontend/src/api/index.ts`)

```ts
fetchCart(): Promise<CartItem[]>              // GET /api/cart
updateCartItem(productId, quantity): Promise<CartItem | null>  // PUT /api/cart/{productId}
removeFromCart(productId): Promise<void>      // DELETE /api/cart/{productId}
```

`CartItem` should be moved out of the local scope in `api/index.ts` and exported so it can be used by the new `CartDrawer` component.

### New component: `CartDrawer`

**Location:** `src/frontend/src/components/CartDrawer/CartDrawer.tsx`

| Prop | Type | Description |
|------|------|-------------|
| `isOpen` | `boolean` | Controls visibility |
| `onClose` | `() => void` | Called when the user dismisses the drawer |
| `items` | `CartItem[]` | Current cart contents |
| `onUpdateQuantity` | `(productId: number, quantity: number) => void` | Called when the user changes an item's quantity (0 = remove) |
| `onClearCart` | `() => void` | Called when the user clears the cart |

**Rendered content:**

- Slide-in panel from the right, overlaying the page (not pushing content)
- A close button (×) in the top-right corner of the drawer
- A backdrop overlay behind the drawer; clicking it closes the drawer
- Empty state message when cart is empty: `"Your cart is empty"`
- Per item row: product name, unit price, quantity selector (− / number / +), per-item total (`unitPrice × quantity`)
  - Pressing − when quantity is 1 removes the item (calls `onUpdateQuantity(id, 0)`)
  - Pressing + when quantity is 5 is disabled (max enforced client-side too)
- Cart total line: `Total: $X.XX` (sum of all `totalPrice` values)
- "Clear cart" button — disabled when cart is empty

### `Header` changes

Add `onCartOpen: () => void` prop. Wire the existing cart `<button>` `onClick` to it.

### `App` changes

1. Add `isCartOpen` state (`boolean`, default `false`)
2. Add `cartItems` state (`CartItem[]`, default `[]`)
3. On mount and after every `addToCart` / `updateCartItem` / `removeFromCart` call: re-fetch cart via `fetchCart()` and update `cartItems`; derive `cartItemCount` from `cartItems.reduce((sum, i) => sum + i.quantity, 0)` (replaces the manual increment)
4. Pass `onCartOpen={() => setIsCartOpen(true)}` to `Header`
5. Render `<CartDrawer>` with `isOpen`, `onClose`, `items`, `onUpdateQuantity`, `onClearCart`

---

## Edge Cases

| Scenario | Expected behaviour |
|----------|--------------------|
| Add same product twice | Quantity increments; still one row in cart |
| Add 3, then add 3 more of same product | Second `POST` returns `400` — would exceed 5 |
| PUT with quantity 5 | Accepted → `200 OK` |
| PUT with quantity 6 | `400 ValidationProblem` |
| PUT with quantity 0 | Item removed → `204 NoContent` |
| PUT for a product not in cart | `404 NotFound` |
| DELETE for a product not in cart | `404 NotFound` |
| GET cart when empty | `200 OK` with `[]` |
| Add product with invalid productId | `404 NotFound` from `POST /api/cart` |
| Clicking backdrop closes drawer | Drawer closes; cart state preserved |
| Cart icon badge | Shows total quantity across all items (not unique item count) |
