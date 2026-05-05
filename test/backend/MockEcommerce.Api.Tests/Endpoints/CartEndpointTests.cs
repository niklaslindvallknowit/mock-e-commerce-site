using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using MockEcommerce.Api.Models;

namespace MockEcommerce.Api.Tests.Endpoints;

public class CartEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CartEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    // ── GET /api/cart ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCart_WhenEmpty_ReturnsOkWithEmptyArray()
    {
        var client = NewClient();
        var response = await client.GetAsync("/api/cart");

        response.EnsureSuccessStatusCode();
        var items = await response.Content.ReadFromJsonAsync<List<CartItem>>();
        Assert.NotNull(items);
        Assert.Empty(items);
    }

    // ── POST /api/cart ────────────────────────────────────────────────────────

    [Fact]
    public async Task AddToCart_WithValidProduct_ReturnsCreated()
    {
        var client = NewClient();
        var response = await client.PostAsJsonAsync("/api/cart", new { productId = 1, quantity = 1 });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var item = await response.Content.ReadFromJsonAsync<CartItem>();
        Assert.NotNull(item);
        Assert.Equal(1, item.ProductId);
        Assert.Equal(1, item.Quantity);
    }

    [Fact]
    public async Task AddToCart_SameProductTwice_ReturnsOkAndIncrements()
    {
        var client = NewClient();
        await client.PostAsJsonAsync("/api/cart", new { productId = 1, quantity = 1 });
        var response = await client.PostAsJsonAsync("/api/cart", new { productId = 1, quantity = 2 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var item = await response.Content.ReadFromJsonAsync<CartItem>();
        Assert.NotNull(item);
        Assert.Equal(3, item.Quantity);
    }

    [Fact]
    public async Task AddToCart_ExceedingMaxQuantity_ReturnsBadRequest()
    {
        var client = NewClient();
        await client.PostAsJsonAsync("/api/cart", new { productId = 1, quantity = 3 });
        var response = await client.PostAsJsonAsync("/api/cart", new { productId = 1, quantity = 3 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddToCart_ExceedingMaxQuantity_PreservesOriginalQuantity()
    {
        var client = NewClient();
        await client.PostAsJsonAsync("/api/cart", new { productId = 1, quantity = 3 });
        await client.PostAsJsonAsync("/api/cart", new { productId = 1, quantity = 3 }); // rejected

        var cart = await client.GetFromJsonAsync<List<CartItem>>("/api/cart");
        Assert.NotNull(cart);
        var item = cart.Single(x => x.ProductId == 1);
        Assert.Equal(3, item.Quantity); // unchanged
    }

    [Fact]
    public async Task AddToCart_WithInvalidProductId_ReturnsNotFound()
    {
        var client = NewClient();
        var response = await client.PostAsJsonAsync("/api/cart", new { productId = 9999, quantity = 1 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddToCart_WithZeroQuantity_ReturnsBadRequest()
    {
        var client = NewClient();
        var response = await client.PostAsJsonAsync("/api/cart", new { productId = 1, quantity = 0 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── PUT /api/cart/{productId} ─────────────────────────────────────────────

    [Fact]
    public async Task UpdateCartItem_SetValidQuantity_ReturnsOk()
    {
        var client = NewClient();
        await client.PostAsJsonAsync("/api/cart", new { productId = 1, quantity = 1 });

        var response = await client.PutAsJsonAsync("/api/cart/1", new { quantity = 4 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var item = await response.Content.ReadFromJsonAsync<CartItem>();
        Assert.NotNull(item);
        Assert.Equal(4, item.Quantity);
    }

    [Fact]
    public async Task UpdateCartItem_SetQuantityToZero_ReturnsNoContentAndRemoves()
    {
        var client = NewClient();
        await client.PostAsJsonAsync("/api/cart", new { productId = 1, quantity = 2 });

        var response = await client.PutAsJsonAsync("/api/cart/1", new { quantity = 0 });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var cart = await client.GetFromJsonAsync<List<CartItem>>("/api/cart");
        Assert.NotNull(cart);
        Assert.DoesNotContain(cart, x => x.ProductId == 1);
    }

    [Fact]
    public async Task UpdateCartItem_ExceedMaxQuantity_ReturnsBadRequest()
    {
        var client = NewClient();
        await client.PostAsJsonAsync("/api/cart", new { productId = 1, quantity = 1 });

        var response = await client.PutAsJsonAsync("/api/cart/1", new { quantity = 6 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCartItem_NegativeQuantity_ReturnsBadRequest()
    {
        var client = NewClient();
        await client.PostAsJsonAsync("/api/cart", new { productId = 1, quantity = 1 });

        var response = await client.PutAsJsonAsync("/api/cart/1", new { quantity = -1 });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCartItem_ItemNotInCart_ReturnsNotFound()
    {
        var client = NewClient();
        var response = await client.PutAsJsonAsync("/api/cart/1", new { quantity = 2 });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── DELETE /api/cart/{productId} ──────────────────────────────────────────

    [Fact]
    public async Task RemoveFromCart_ExistingItem_ReturnsNoContent()
    {
        var client = NewClient();
        await client.PostAsJsonAsync("/api/cart", new { productId = 1, quantity = 1 });

        var response = await client.DeleteAsync("/api/cart/1");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task RemoveFromCart_NonExistentItem_ReturnsNotFound()
    {
        var client = NewClient();
        var response = await client.DeleteAsync("/api/cart/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── DELETE /api/cart ──────────────────────────────────────────────────────

    [Fact]
    public async Task ClearCart_RemovesAllItems_ReturnsNoContent()
    {
        var client = NewClient();
        await client.PostAsJsonAsync("/api/cart", new { productId = 1, quantity = 1 });
        await client.PostAsJsonAsync("/api/cart", new { productId = 2, quantity = 1 });

        var response = await client.DeleteAsync("/api/cart");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var cart = await client.GetFromJsonAsync<List<CartItem>>("/api/cart");
        Assert.NotNull(cart);
        Assert.Empty(cart);
    }

    // Each test gets its own isolated WebApplicationFactory so the singleton cart starts empty.
    private static HttpClient NewClient() =>
        new WebApplicationFactory<Program>().CreateClient();
}
