using Microsoft.AspNetCore.Http.HttpResults;
using MockEcommerce.Api.Models;
using MockEcommerce.Api.Services;

namespace MockEcommerce.Api.Endpoints;

/// <summary>
/// Maps shopping cart endpoints under <c>/api/cart</c>.
/// </summary>
public static class CartEndpoints
{
    private const int MaxQuantityPerItem = 5;

    /// <summary>Registers cart-related routes on the given endpoint route builder.</summary>
    public static void MapCartEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("api/cart")
            .WithTags("Cart");

        group.MapGet("/", GetCart)
            .WithName("GetCart")
            .WithSummary("Returns all items currently in the cart.");

        group.MapPost("/", AddToCart)
            .WithName("AddToCart")
            .WithSummary("Adds a product to the cart or increments quantity if already present.");

        group.MapPut("/{productId:int}", UpdateCartItem)
            .WithName("UpdateCartItem")
            .WithSummary("Sets the quantity of a cart item, or removes it when quantity is 0.");

        group.MapDelete("/{productId:int}", RemoveFromCart)
            .WithName("RemoveFromCart")
            .WithSummary("Removes a single product from the cart by its product ID.");

        group.MapDelete("/", ClearCart)
            .WithName("ClearCart")
            .WithSummary("Removes all items from the cart.");
    }

    /// <summary>Returns all items currently in the cart.</summary>
    internal static Ok<IEnumerable<CartItem>> GetCart(ICartService cartService)
    {
        return TypedResults.Ok(cartService.GetAll());
    }

    /// <summary>Adds a product to the cart or increments quantity if already present.</summary>
    internal static Results<Created<CartItem>, Ok<CartItem>, NotFound<string>, ValidationProblem> AddToCart(
        AddToCartRequest request,
        IProductService productService,
        ICartService cartService)
    {
        if (request.Quantity <= 0)
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]> { ["quantity"] = ["Quantity must be at least 1"] });

        var product = productService.GetById(request.ProductId);
        if (product is null)
            return TypedResults.NotFound($"Product {request.ProductId} not found");

        var existing = cartService.GetByProductId(request.ProductId);
        var existingQty = existing?.Quantity ?? 0;

        if (existingQty + request.Quantity > MaxQuantityPerItem)
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]> { ["quantity"] = [$"Cannot exceed {MaxQuantityPerItem} of any single item"] });

        var cartItem = new CartItem
        {
            ProductId = product.Id,
            ProductName = product.Name,
            UnitPrice = product.Price,
            Quantity = request.Quantity
        };

        var saved = cartService.Add(cartItem);

        return existingQty == 0
            ? TypedResults.Created($"/api/cart/{saved.ProductId}", saved)
            : TypedResults.Ok(saved);
    }

    /// <summary>Sets the quantity of a cart item, or removes it when quantity is 0.</summary>
    internal static Results<Ok<CartItem>, NoContent, NotFound, ValidationProblem> UpdateCartItem(
        int productId,
        UpdateCartItemRequest request,
        ICartService cartService)
    {
        if (request.Quantity < 0)
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]> { ["quantity"] = ["Quantity must be 0 or greater"] });

        var existing = cartService.GetByProductId(productId);
        if (existing is null)
            return TypedResults.NotFound();

        if (request.Quantity == 0)
        {
            cartService.Remove(productId);
            return TypedResults.NoContent();
        }

        if (request.Quantity > MaxQuantityPerItem)
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]> { ["quantity"] = [$"Quantity cannot exceed {MaxQuantityPerItem}"] });

        var updated = cartService.Update(productId, request.Quantity);
        return TypedResults.Ok(updated);
    }

    /// <summary>Removes a single product from the cart by its product ID.</summary>
    internal static Results<NoContent, NotFound> RemoveFromCart(int productId, ICartService cartService)
    {
        return cartService.Remove(productId)
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }

    /// <summary>Removes all items from the cart.</summary>
    internal static NoContent ClearCart(ICartService cartService)
    {
        cartService.Clear();
        return TypedResults.NoContent();
    }
}

/// <summary>Request body for adding a product to the cart.</summary>
public record AddToCartRequest(int ProductId, int Quantity);

/// <summary>Request body for updating the quantity of a cart item.</summary>
public record UpdateCartItemRequest(int Quantity);
