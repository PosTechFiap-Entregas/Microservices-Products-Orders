using AutoFixture;
using FluentAssertions;
using Moq;
using Products.Application.DTOs;
using Products.Application.Services.Service;
using Products.Domain.Entities;
using Products.Domain.Enums;
using Products.Domain.Interfaces.Repository;

namespace Products.Tests.Application.Services.Service;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly ProductService _service;
    private readonly Fixture _fixture;

    public ProductServiceTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _service = new ProductService(_repositoryMock.Object);
        _fixture = new Fixture();
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductExists_ReturnsProductDto()
    {
        var product = _fixture.Build<Product>()
            .With(p => p.Id, 1)
            .With(p => p.Name, "X-Burger")
            .With(p => p.Price, 25.90m)
            .With(p => p.Category, CategoryEnum.SANDWICH)
            .With(p => p.Active, true)
            .Create();

        _repositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);

        var result = await _service.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("X-Burger");
        result.Price.Should().Be(25.90m);
        result.Category.Should().Be(CategoryEnum.SANDWICH);
        result.Active.Should().BeTrue();

        _repositoryMock.Verify(r => r.GetByIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductDoesNotExist_ReturnsNull()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);

        var result = await _service.GetByIdAsync(999);

        result.Should().BeNull();
        _repositoryMock.Verify(r => r.GetByIdAsync(999), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllProducts()
    {
        var products = _fixture.Build<Product>()
            .With(p => p.Active, true)
            .CreateMany(5)
            .ToList();

        _repositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(products);

        var result = await _service.GetAllAsync();

        result.Should().HaveCount(5);
        result.Should().AllSatisfy(p => p.Active.Should().BeTrue());
        _repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByCategoryAsync_ReturnOnlyProductsOfCategory()
    {
        var sandwiches = _fixture.Build<Product>()
            .With(p => p.Category, CategoryEnum.SANDWICH)
            .CreateMany(3)
            .ToList();

        _repositoryMock.Setup(r => r.GetByCategoryAsync(CategoryEnum.SANDWICH))
            .ReturnsAsync(sandwiches);

        var result = await _service.GetByCategoryAsync(CategoryEnum.SANDWICH);

        result.Should().HaveCount(3);
        result.Should().AllSatisfy(p =>
            p.Category.Should().Be(CategoryEnum.SANDWICH));
        _repositoryMock.Verify(r => r.GetByCategoryAsync(CategoryEnum.SANDWICH), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_CreatesAndReturnsProduct()
    {
        var createDto = new CreateProductDto(
            Name: "X-Bacon",
            Price: 29.90m,
            Category: CategoryEnum.SANDWICH,
            Description: "Delicioso hambúrguer",
            ImageUrl: "https://example.com/bacon.jpg"
        );

        var createdProduct = new Product
        {
            Id = 1,
            Name = createDto.Name,
            Price = createDto.Price,
            Category = createDto.Category,
            Description = createDto.Description,
            ImageUrl = createDto.ImageUrl,
            Active = true
        };

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync(createdProduct);

        var result = await _service.CreateAsync(createDto);

        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("X-Bacon");
        result.Price.Should().Be(29.90m);
        result.Active.Should().BeTrue();

        _repositoryMock.Verify(r => r.AddAsync(It.Is<Product>(p =>
            p.Name == createDto.Name &&
            p.Price == createDto.Price &&
            p.Category == createDto.Category
        )), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenProductExists_UpdatesAndReturnsProduct()
    {
        var existingProduct = _fixture.Build<Product>()
            .With(p => p.Id, 1)
            .With(p => p.Name, "X-Burger")
            .With(p => p.Price, 25.90m)
            .Create();

        var updateDto = new UpdateProductDto(
            Name: "X-Burger Premium",
            Price: 32.90m,
            Category: CategoryEnum.SANDWICH,
            Description: "Versão premium",
            Active: true,
            ImageUrl: null
        );

        _repositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(existingProduct);

        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>()))
            .ReturnsAsync((Product p) => p);

        var result = await _service.UpdateAsync(1, updateDto);

        result.Should().NotBeNull();
        result.Name.Should().Be("X-Burger Premium");
        result.Price.Should().Be(32.90m);

        _repositoryMock.Verify(r => r.GetByIdAsync(1), Times.Once);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenProductDoesNotExist_ThrowsKeyNotFoundException()
    {
        var updateDto = _fixture.Create<UpdateProductDto>();

        _repositoryMock.Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);

        Func<Task> act = async () => await _service.UpdateAsync(999, updateDto);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*999*");

        _repositoryMock.Verify(r => r.GetByIdAsync(999), Times.Once);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenProductExists_ReturnsTrue()
    {
        _repositoryMock.Setup(r => r.DeleteAsync(1))
            .ReturnsAsync(true);

        var result = await _service.DeleteAsync(1);

        result.Should().BeTrue();
        _repositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenProductDoesNotExist_ReturnsFalse()
    {
        _repositoryMock.Setup(r => r.DeleteAsync(999))
            .ReturnsAsync(false);

        var result = await _service.DeleteAsync(999);

        result.Should().BeFalse();
        _repositoryMock.Verify(r => r.DeleteAsync(999), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithNegativePrice_ThrowsArgumentException()
    {
        var existingProduct = _fixture.Build<Product>()
            .With(p => p.Id, 2)
            .With(p => p.Name, "X-Test")
            .With(p => p.Price, 10.00m)
            .Create();

        var updateDto = new UpdateProductDto(
            Name: "X-Test",
            Price: -5.00m,
            Category: CategoryEnum.SANDWICH,
            Description: "Desc",
            Active: true,
            ImageUrl: null
        );

        _repositoryMock.Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(existingProduct);

        Func<Task> act = async () => await _service.UpdateAsync(2, updateDto);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("O preço não pode ser negativo");

        _repositoryMock.Verify(r => r.GetByIdAsync(2), Times.Once);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidName_ThrowsArgumentException()
    {
        var existingProduct = _fixture.Build<Product>()
            .With(p => p.Id, 3)
            .With(p => p.Name, "Original")
            .With(p => p.Price, 20.00m)
            .Create();

        var updateDto = new UpdateProductDto(
            Name: "", // invalid
            Price: 20.00m,
            Category: CategoryEnum.SANDWICH,
            Description: "Desc",
            Active: true,
            ImageUrl: null
        );

        _repositoryMock.Setup(r => r.GetByIdAsync(3))
            .ReturnsAsync(existingProduct);

        Func<Task> act = async () => await _service.UpdateAsync(3, updateDto);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("O nome do produto é obrigatório");

        _repositoryMock.Verify(r => r.GetByIdAsync(3), Times.Once);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenRepositoryThrows_PropagatesException()
    {
        var createDto = new CreateProductDto("Fail", 1.00m, CategoryEnum.DRINK, null, null);

        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Product>()))
            .ThrowsAsync(new Exception("DB failure"));

        Func<Task> act = async () => await _service.CreateAsync(createDto);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*DB failure*");

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_MapsCreatedAtAndUpdatedAt()
    {
        var now = DateTime.UtcNow;
        var p1 = new Product { Id = 10, Name = "A", Price = 1m, Category = CategoryEnum.SANDWICH, Active = true, CreatedAt = now.AddDays(-1), UpdatedAt = now };
        var p2 = new Product { Id = 11, Name = "B", Price = 2m, Category = CategoryEnum.SIDE, Active = true, CreatedAt = now.AddDays(-2), UpdatedAt = now.AddHours(-1) };

        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product> { p1, p2 });

        var result = (await _service.GetAllAsync()).ToList();

        result.Should().HaveCount(2);
        result[0].CreatedAt.Should().Be(p1.CreatedAt);
        result[0].UpdatedAt.Should().Be(p1.UpdatedAt);
        result[1].CreatedAt.Should().Be(p2.CreatedAt);
        result[1].UpdatedAt.Should().Be(p2.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_AppliesAllFields_ReturnsUpdatedDto()
    {
        var existingProduct = new Product { Id = 4, Name = "Old", Price = 5m, Category = CategoryEnum.SANDWICH, Active = true, Description = "Old", ImageUrl = "old.jpg" };
        var updateDto = new UpdateProductDto("NewName", 15.00m, CategoryEnum.DRINK, "NewDesc", false, "new.jpg");

        _repositoryMock.Setup(r => r.GetByIdAsync(4)).ReturnsAsync(existingProduct);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Product>())).ReturnsAsync((Product p) => p);

        var result = await _service.UpdateAsync(4, updateDto);

        result.Should().NotBeNull();
        result.Name.Should().Be("NewName");
        result.Price.Should().Be(15.00m);
        result.Category.Should().Be(CategoryEnum.DRINK);
        result.Description.Should().Be("NewDesc");
        result.Active.Should().BeFalse();
        result.ImageUrl.Should().Be("new.jpg");

        _repositoryMock.Verify(r => r.GetByIdAsync(4), Times.Once);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Once);
    }
}