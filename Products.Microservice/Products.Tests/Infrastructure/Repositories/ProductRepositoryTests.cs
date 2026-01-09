using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Products.Domain.Entities;
using Products.Domain.Enums;
using Products.Infrastructure.Data;
using Products.Infrastructure.Repositories;

namespace Products.Tests.Infrastructure.Repositories;

public class ProductRepositoryTests : IDisposable
{
    private readonly ProductsDbContext _context;
    private readonly ProductRepository _repository;

    public ProductRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ProductsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ProductsDbContext(options);
        _repository = new ProductRepository(_context);
    }

    [Fact]
    public async Task AddAsync_AddsProductToDatabase()
    {
        var product = new Product
        {
            Name = "X-Burger",
            Price = 25.90m,
            Category = CategoryEnum.SANDWICH,
            Active = true
        };

        var result = await _repository.AddAsync(product);

        result.Id.Should().BeGreaterThan(0);

        var savedProduct = await _context.Products.FindAsync(result.Id);
        savedProduct.Should().NotBeNull();
        savedProduct!.Name.Should().Be("X-Burger");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsProduct_WhenExists()
    {
        var product = new Product
        {
            Name = "X-Bacon",
            Price = 29.90m,
            Category = CategoryEnum.SANDWICH,
            Active = true
        };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(product.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("X-Bacon");
        result.Price.Should().Be(29.90m);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        var result = await _repository.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCategoryAsync_ReturnsOnlyProductsOfCategory()
    {
        await _context.Products.AddRangeAsync(
            new Product { Name = "X-Burger", Price = 25.90m, Category = CategoryEnum.SANDWICH, Active = true },
            new Product { Name = "Batata", Price = 12.90m, Category = CategoryEnum.SIDE, Active = true },
            new Product { Name = "X-Bacon", Price = 29.90m, Category = CategoryEnum.SANDWICH, Active = true }
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetByCategoryAsync(CategoryEnum.SANDWICH);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(p => p.Category.Should().Be(CategoryEnum.SANDWICH));
    }

    [Fact]
    public async Task GetActiveProductsAsync_ReturnsOnlyActiveProducts()
    {
        await _context.Products.AddRangeAsync(
            new Product { Name = "Active1", Price = 10m, Category = CategoryEnum.SANDWICH, Active = true },
            new Product { Name = "Inactive", Price = 10m, Category = CategoryEnum.SANDWICH, Active = false },
            new Product { Name = "Active2", Price = 10m, Category = CategoryEnum.SIDE, Active = true }
        );
        await _context.SaveChangesAsync();

        var result = await _repository.GetActiveProductsAsync();

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(p => p.Active.Should().BeTrue());
    }

    [Fact]
    public async Task UpdateAsync_UpdatesProduct()
    {
        var product = new Product
        {
            Name = "Original",
            Price = 10m,
            Category = CategoryEnum.SANDWICH,
            Active = true
        };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        product.Name = "Updated";
        product.Price = 20m;
        await _repository.UpdateAsync(product);

        var updated = await _context.Products.FindAsync(product.Id);
        updated!.Name.Should().Be("Updated");
        updated.Price.Should().Be(20m);
    }

    [Fact]
    public async Task DeleteAsync_RemovesProduct()
    {
        var product = new Product
        {
            Name = "To Delete",
            Price = 10m,
            Category = CategoryEnum.SANDWICH,
            Active = true
        };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
        var productId = product.Id;

        var result = await _repository.DeleteAsync(productId);

        result.Should().BeTrue();
        var deleted = await _context.Products.FindAsync(productId);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_ReturnsTrue_WhenProductExists()
    {
        var product = new Product
        {
            Name = "Exists",
            Price = 10m,
            Category = CategoryEnum.SANDWICH,
            Active = true
        };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var result = await _repository.ExistsAsync(product.Id);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ReturnsFalse_WhenProductDoesNotExist()
    {
        var result = await _repository.ExistsAsync(999);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_ShouldOrderByCategoryThenName()
    {
        await _context.Products.AddRangeAsync(new[]
        {
            new Product { Name = "B-Item", Price = 1m, Category = CategoryEnum.DRINK, Active = true },
            new Product { Name = "A-Item", Price = 2m, Category = CategoryEnum.SANDWICH, Active = true },
            new Product { Name = "C-Item", Price = 3m, Category = CategoryEnum.DRINK, Active = true },
            new Product { Name = "Z-Item", Price = 4m, Category = CategoryEnum.SIDE, Active = true },
            new Product { Name = "AA-Item", Price = 5m, Category = CategoryEnum.SANDWICH, Active = true },
        });
        await _context.SaveChangesAsync();

        var result = (await _repository.GetAllAsync()).ToList();

        var expected = result.OrderBy(p => p.Category).ThenBy(p => p.Name).Select(p => (p.Category, p.Name)).ToList();
        result.Select(p => (p.Category, p.Name)).Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task GetByCategoryAsync_ShouldReturnOrderedByName()
    {
        var category = CategoryEnum.SANDWICH;
        await _context.Products.AddRangeAsync(new[]
        {
            new Product { Name = "Zeta", Price = 1m, Category = category, Active = true },
            new Product { Name = "Alpha", Price = 2m, Category = category, Active = true },
            new Product { Name = "Beta", Price = 3m, Category = category, Active = true }
        });
        await _context.SaveChangesAsync();

        var result = (await _repository.GetByCategoryAsync(category)).ToList();

        var names = result.Select(p => p.Name).ToList();
        names.Should().ContainInOrder("Alpha", "Beta", "Zeta");
    }

    [Fact]
    public async Task UpdateAsync_ShouldSetUpdatedAtToNewerValue()
    {
        var product = new Product
        {
            Name = "TimestampTest",
            Price = 10m,
            Category = CategoryEnum.SANDWICH,
            Active = true,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow.AddDays(-2)
        };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var originalUpdatedAt = product.UpdatedAt;

        product.Price = 20m;
        await _repository.UpdateAsync(product);

        var fromDb = await _context.Products.FindAsync(product.Id);

        fromDb.Should().NotBeNull();
        fromDb!.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}