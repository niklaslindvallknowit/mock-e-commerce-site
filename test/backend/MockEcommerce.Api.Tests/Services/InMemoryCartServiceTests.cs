using MockEcommerce.Api.Models;
using MockEcommerce.Api.Services;

namespace MockEcommerce.Api.Tests.Services;

public class InMemoryCartServiceTests
{
    private InMemoryCartService CreateService() => new();

    private static CartItem MakeItem(int productId, string name = "Test", decimal price = 9.99m, int qty = 1) =>
        new() { ProductId = productId, ProductName = name, UnitPrice = price, Quantity = qty };

    [Fact]
    public void GetAll_WhenEmpty_ReturnsEmptyList()
    {
        var service = CreateService();
        Assert.Empty(service.GetAll());
    }

    [Fact]
    public void Add_NewItem_ReturnsItemAndPersists()
    {
        var service = CreateService();
        var item = MakeItem(1);

        var result = service.Add(item);

        Assert.Equal(1, result.ProductId);
        Assert.Single(service.GetAll());
    }

    [Fact]
    public void Add_ExistingItem_IncrementsQuantity()
    {
        var service = CreateService();
        service.Add(MakeItem(1, qty: 2));

        var result = service.Add(MakeItem(1, qty: 3));

        Assert.Equal(5, result.Quantity);
        Assert.Single(service.GetAll());
    }

    [Fact]
    public void GetByProductId_WithValidId_ReturnsItem()
    {
        var service = CreateService();
        service.Add(MakeItem(42));

        var result = service.GetByProductId(42);

        Assert.NotNull(result);
        Assert.Equal(42, result.ProductId);
    }

    [Fact]
    public void GetByProductId_WithInvalidId_ReturnsNull()
    {
        var service = CreateService();
        Assert.Null(service.GetByProductId(999));
    }

    [Fact]
    public void Remove_ExistingItem_ReturnsTrueAndRemoves()
    {
        var service = CreateService();
        service.Add(MakeItem(1));

        var removed = service.Remove(1);

        Assert.True(removed);
        Assert.Empty(service.GetAll());
    }

    [Fact]
    public void Remove_NonExistentItem_ReturnsFalse()
    {
        var service = CreateService();
        Assert.False(service.Remove(999));
    }

    [Fact]
    public void Clear_RemovesAllItems()
    {
        var service = CreateService();
        service.Add(MakeItem(1));
        service.Add(MakeItem(2));

        service.Clear();

        Assert.Empty(service.GetAll());
    }

    [Fact]
    public void Update_ExistingItem_SetsQuantity()
    {
        var service = CreateService();
        service.Add(MakeItem(1, qty: 1));

        var result = service.Update(1, 4);

        Assert.Equal(4, result.Quantity);
        Assert.Equal(4, service.GetByProductId(1)!.Quantity);
    }
}
