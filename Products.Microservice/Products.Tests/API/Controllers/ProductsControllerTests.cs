using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Products.API.Controllers;
using Products.Application.DTOs;
using Products.Application.Services.Interface;
using Products.Domain.Enums;

namespace Products.Tests.API.Controllers
{
    public class ProductsControllerTests
    {
        private readonly Mock<IProductService> _serviceMock;
        private readonly Mock<ILogger<ProductsController>> _loggerMock;
        private readonly ProductsController _controller;

        public ProductsControllerTests()
        {
            _serviceMock = new Mock<IProductService>();
            _loggerMock = new Mock<ILogger<ProductsController>>();
            _controller = new ProductsController(_serviceMock.Object, _loggerMock.Object);
        }

        #region GetAll Tests

        [Fact]
        public async Task GetAll_WithProducts_ReturnsOkWithAllProducts()
        {
            var products = new List<ProductDto>
            {
                CreateSampleProductDto(1, "Produto 1"),
                CreateSampleProductDto(2, "Produto 2"),
                CreateSampleProductDto(3, "Produto 3")
            };

            _serviceMock.Setup(s => s.GetAllAsync())
                .ReturnsAsync(products);

            var result = await _controller.GetAll();

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedProducts = okResult.Value.Should().BeAssignableTo<IEnumerable<ProductDto>>().Subject;
            returnedProducts.Should().HaveCount(3);
            returnedProducts.Should().BeEquivalentTo(products);

            _serviceMock.Verify(s => s.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAll_WithNoProducts_ReturnsOkWithEmptyList()
        {
            _serviceMock.Setup(s => s.GetAllAsync())
                .ReturnsAsync(new List<ProductDto>());

            var result = await _controller.GetAll();

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedProducts = okResult.Value.Should().BeAssignableTo<IEnumerable<ProductDto>>().Subject;
            returnedProducts.Should().BeEmpty();

            _serviceMock.Verify(s => s.GetAllAsync(), Times.Once);
        }

        #endregion

        #region GetById Tests

        [Fact]
        public async Task GetById_WithExistingProduct_ReturnsOkWithProduct()
        {
            var product = CreateSampleProductDto(1, "Produto Teste");

            _serviceMock.Setup(s => s.GetByIdAsync(1))
                .ReturnsAsync(product);

            var result = await _controller.GetById(1);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedProduct = okResult.Value.Should().BeAssignableTo<ProductDto>().Subject;
            returnedProduct.Should().BeEquivalentTo(product);

            _serviceMock.Verify(s => s.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetById_WithNonExistingProduct_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(999))
                .ReturnsAsync((ProductDto?)null);

            var result = await _controller.GetById(999);

            var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().BeEquivalentTo(new { message = "Produto com ID 999 não encontrado" });

            _serviceMock.Verify(s => s.GetByIdAsync(999), Times.Once);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(100)]
        public async Task GetById_WithDifferentValidIds_ReturnsOk(int id)
        {
            var product = CreateSampleProductDto(id, $"Produto {id}");

            _serviceMock.Setup(s => s.GetByIdAsync(id))
                .ReturnsAsync(product);

            var result = await _controller.GetById(id);

            result.Result.Should().BeOfType<OkObjectResult>();
            _serviceMock.Verify(s => s.GetByIdAsync(id), Times.Once);
        }

        #endregion

        #region GetActive Tests

        [Fact]
        public async Task GetActive_WithActiveProducts_ReturnsOkWithActiveProducts()
        {
            var activeProducts = new List<ProductDto>
            {
                CreateSampleProductDto(1, "Produto Ativo 1", active: true),
                CreateSampleProductDto(2, "Produto Ativo 2", active: true)
            };

            _serviceMock.Setup(s => s.GetActiveProductsAsync())
                .ReturnsAsync(activeProducts);

            var result = await _controller.GetActive();

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedProducts = okResult.Value.Should().BeAssignableTo<IEnumerable<ProductDto>>().Subject;
            returnedProducts.Should().HaveCount(2);
            returnedProducts.All(p => p.Active).Should().BeTrue();

            _serviceMock.Verify(s => s.GetActiveProductsAsync(), Times.Once);
        }

        [Fact]
        public async Task GetActive_WithNoActiveProducts_ReturnsOkWithEmptyList()
        {
            _serviceMock.Setup(s => s.GetActiveProductsAsync())
                .ReturnsAsync(new List<ProductDto>());

            var result = await _controller.GetActive();

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedProducts = okResult.Value.Should().BeAssignableTo<IEnumerable<ProductDto>>().Subject;
            returnedProducts.Should().BeEmpty();

            _serviceMock.Verify(s => s.GetActiveProductsAsync(), Times.Once);
        }

        #endregion

        #region GetByCategory Tests

        [Theory]
        [InlineData(CategoryEnum.SANDWICH)]
        [InlineData(CategoryEnum.SIDE)]
        [InlineData(CategoryEnum.DRINK)]
        [InlineData(CategoryEnum.DESSERT)]
        public async Task GetByCategory_WithSpecificCategory_ReturnsOkWithFilteredProducts(CategoryEnum category)
        {
            var products = new List<ProductDto>
            {
                CreateSampleProductDto(1, "Produto 1", category: category),
                CreateSampleProductDto(2, "Produto 2", category: category)
            };

            _serviceMock.Setup(s => s.GetByCategoryAsync(category))
                .ReturnsAsync(products);

            var result = await _controller.GetByCategory(category);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedProducts = okResult.Value.Should().BeAssignableTo<IEnumerable<ProductDto>>().Subject;
            returnedProducts.Should().HaveCount(2);
            returnedProducts.All(p => p.Category == category).Should().BeTrue();

            _serviceMock.Verify(s => s.GetByCategoryAsync(category), Times.Once);
        }

        [Fact]
        public async Task GetByCategory_WithNoProductsInCategory_ReturnsOkWithEmptyList()
        {
            _serviceMock.Setup(s => s.GetByCategoryAsync(CategoryEnum.SANDWICH))
                .ReturnsAsync(new List<ProductDto>());

            var result = await _controller.GetByCategory(CategoryEnum.SANDWICH);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedProducts = okResult.Value.Should().BeAssignableTo<IEnumerable<ProductDto>>().Subject;
            returnedProducts.Should().BeEmpty();

            _serviceMock.Verify(s => s.GetByCategoryAsync(CategoryEnum.SANDWICH), Times.Once);
        }

        #endregion

        #region Create Tests

        [Fact]
        public async Task Create_WithValidData_ReturnsCreatedAtAction()
        {
            var createDto = new CreateProductDto("Novo Produto", 50.00m, CategoryEnum.SANDWICH, "Descrição", null);
            var createdProduct = CreateSampleProductDto(1, "Novo Produto");

            _serviceMock.Setup(s => s.CreateAsync(createDto))
                .ReturnsAsync(createdProduct);

            var result = await _controller.Create(createDto);

            var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(ProductsController.GetById));
            createdResult.RouteValues.Should().ContainKey("id");
            createdResult.RouteValues!["id"].Should().Be(1);

            var returnedProduct = createdResult.Value.Should().BeAssignableTo<ProductDto>().Subject;
            returnedProduct.Should().BeEquivalentTo(createdProduct);

            _serviceMock.Verify(s => s.CreateAsync(createDto), Times.Once);
        }

        [Fact]
        public async Task Create_WhenServiceThrowsException_ReturnsBadRequest()
        {
            var createDto = new CreateProductDto("Produto Inválido", -10.00m, CategoryEnum.SANDWICH, null, null);

            _serviceMock.Setup(s => s.CreateAsync(createDto))
                .ThrowsAsync(new ArgumentException("Preço não pode ser negativo"));

            var result = await _controller.Create(createDto);

            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeEquivalentTo(new { message = "Preço não pode ser negativo" });

            _serviceMock.Verify(s => s.CreateAsync(createDto), Times.Once);
        }

        [Fact]
        public async Task Create_WhenServiceThrowsException_LogsError()
        {
            var createDto = new CreateProductDto("Produto", 10.00m, CategoryEnum.SANDWICH, null, null);
            var exception = new Exception("Erro ao criar produto");

            _serviceMock.Setup(s => s.CreateAsync(createDto))
                .ThrowsAsync(exception);

            await _controller.Create(createDto);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Erro ao criar produto")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Create_WithMultipleDifferentProducts_ReturnsCreatedForEach()
        {

            var createDto1 = new CreateProductDto("Produto 1", 10.00m, CategoryEnum.SANDWICH, null, null);
            var createDto2 = new CreateProductDto("Produto 2", 20.00m, CategoryEnum.DRINK, null, null);

            var createdProduct1 = CreateSampleProductDto(1, "Produto 1");
            var createdProduct2 = CreateSampleProductDto(2, "Produto 2");

            _serviceMock.Setup(s => s.CreateAsync(createDto1))
                .ReturnsAsync(createdProduct1);
            _serviceMock.Setup(s => s.CreateAsync(createDto2))
                .ReturnsAsync(createdProduct2);

            var result1 = await _controller.Create(createDto1);
            var result2 = await _controller.Create(createDto2);

            result1.Result.Should().BeOfType<CreatedAtActionResult>();
            result2.Result.Should().BeOfType<CreatedAtActionResult>();

            _serviceMock.Verify(s => s.CreateAsync(It.IsAny<CreateProductDto>()), Times.Exactly(2));
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task Update_WithValidData_ReturnsOkWithUpdatedProduct()
        {
            var updateDto = new UpdateProductDto("Produto Atualizado", 75.00m, CategoryEnum.SANDWICH, "Nova descrição", true, null);
            var updatedProduct = CreateSampleProductDto(1, "Produto Atualizado", price: 75.00m);

            _serviceMock.Setup(s => s.UpdateAsync(1, updateDto))
                .ReturnsAsync(updatedProduct);

            var result = await _controller.Update(1, updateDto);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedProduct = okResult.Value.Should().BeAssignableTo<ProductDto>().Subject;
            returnedProduct.Should().BeEquivalentTo(updatedProduct);

            _serviceMock.Verify(s => s.UpdateAsync(1, updateDto), Times.Once);
        }

        [Fact]
        public async Task Update_WithNonExistingProduct_ReturnsNotFound()
        {
            var updateDto = new UpdateProductDto("Produto", 50.00m, CategoryEnum.SANDWICH, null, true, null);

            _serviceMock.Setup(s => s.UpdateAsync(999, updateDto))
                .ThrowsAsync(new KeyNotFoundException("Produto com ID 999 não encontrado"));

            var result = await _controller.Update(999, updateDto);

            var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().BeEquivalentTo(new { message = "Produto com ID 999 não encontrado" });

            _serviceMock.Verify(s => s.UpdateAsync(999, updateDto), Times.Once);
        }

        [Fact]
        public async Task Update_WhenServiceThrowsException_ReturnsBadRequest()
        {
            var updateDto = new UpdateProductDto("Produto", -50.00m, CategoryEnum.SANDWICH, null, true, null);

            _serviceMock.Setup(s => s.UpdateAsync(1, updateDto))
                .ThrowsAsync(new ArgumentException("Preço inválido"));

            var result = await _controller.Update(1, updateDto);

            var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
            badRequestResult.Value.Should().BeEquivalentTo(new { message = "Preço inválido" });

            _serviceMock.Verify(s => s.UpdateAsync(1, updateDto), Times.Once);
        }

        [Fact]
        public async Task Update_WhenServiceThrowsException_LogsError()
        {
            var updateDto = new UpdateProductDto("Produto", 50.00m, CategoryEnum.SANDWICH, null, true, null);
            var exception = new Exception("Erro ao atualizar");

            _serviceMock.Setup(s => s.UpdateAsync(1, updateDto))
                .ThrowsAsync(exception);

            await _controller.Update(1, updateDto);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Erro ao atualizar produto")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        public async Task Update_WithDifferentValidIds_ReturnsOk(int id)
        {
            var updateDto = new UpdateProductDto("Produto", 50.00m, CategoryEnum.SANDWICH, null, true, null);
            var updatedProduct = CreateSampleProductDto(id, "Produto");

            _serviceMock.Setup(s => s.UpdateAsync(id, updateDto))
                .ReturnsAsync(updatedProduct);

            var result = await _controller.Update(id, updateDto);

            result.Result.Should().BeOfType<OkObjectResult>();
            _serviceMock.Verify(s => s.UpdateAsync(id, updateDto), Times.Once);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_WithExistingProduct_ReturnsNoContent()
        {
            _serviceMock.Setup(s => s.DeleteAsync(1))
                .ReturnsAsync(true);

            var result = await _controller.Delete(1);

            result.Should().BeOfType<NoContentResult>();

            _serviceMock.Verify(s => s.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task Delete_WithNonExistingProduct_ReturnsNotFound()
        {
            _serviceMock.Setup(s => s.DeleteAsync(999))
                .ReturnsAsync(false);

            var result = await _controller.Delete(999);

            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            notFoundResult.Value.Should().BeEquivalentTo(new { message = "Produto com ID 999 não encontrado" });

            _serviceMock.Verify(s => s.DeleteAsync(999), Times.Once);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(100)]
        public async Task Delete_WithDifferentValidIds_ReturnsNoContent(int id)
        {
            _serviceMock.Setup(s => s.DeleteAsync(id))
                .ReturnsAsync(true);

            var result = await _controller.Delete(id);

            result.Should().BeOfType<NoContentResult>();
            _serviceMock.Verify(s => s.DeleteAsync(id), Times.Once);
        }

        [Fact]
        public async Task Delete_CalledMultipleTimes_InvokesServiceEachTime()
        {
            _serviceMock.Setup(s => s.DeleteAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            await _controller.Delete(1);
            await _controller.Delete(2);
            await _controller.Delete(3);

            _serviceMock.Verify(s => s.DeleteAsync(It.IsAny<int>()), Times.Exactly(3));
        }

        #endregion

        #region Helper Methods

        private static ProductDto CreateSampleProductDto(
            int id = 1,
            string name = "Produto Teste",
            decimal price = 50.00m,
            CategoryEnum category = CategoryEnum.SANDWICH,
            bool active = true)
        {
            return new ProductDto(
                Id: id,
                Name: name,
                Price: price,
                Category: category,
                Description: "Descrição do produto",
                Active: active,
                ImageUrl: "https://example.com/image.jpg",
                CreatedAt: DateTime.UtcNow,
                UpdatedAt: DateTime.UtcNow
            );
        }

        #endregion
    }
}