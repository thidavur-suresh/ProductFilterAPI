
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Moq;
using Moq.Protected;
using ProductFilter.Core.Models;
using ProductFilter.Core.Services;
using System.Text.Json;
using Xunit;

using IHttpClientFactory = System.Net.Http.IHttpClientFactory;

namespace ProductFilter.Tests;

public class ProductServiceTests
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<ILogger<ProductService>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        
        _mockLogger = new Mock<ILogger<ProductService>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        var client = new HttpClient(_mockHttpMessageHandler.Object);
        _mockHttpClientFactory.Setup(x => x.CreateClient())
            .Returns(client);

        _productService = new ProductService(_mockHttpClientFactory.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetFilteredProducts_WithNoFilters_ReturnsAllProducts()
    {
        // Arrange
        var testProducts = new[]
        {
            new Product { price = 10, sizes = {"Medium"} },
            new Product { price = 20, sizes = {"Large" } }
        };

        SetupMockHttpResponse(testProducts);

        // Act
        var result = await _productService.GetFilteredProducts(null, null, null, null);

        // Assert
        Assert.Equal(2, result.Products.Count());
    }

    [Fact]
    public async Task GetFilteredProducts_WithPriceFilter_ReturnsFilteredProducts()
    {
        // Arrange
        var testProducts = new[]
        {
            new Product {  price = 10, sizes = {"Medium" } },
            new Product {  price = 20, sizes = {"Large" } }
        };

        SetupMockHttpResponse(testProducts);

        // Act
        var result = await _productService.GetFilteredProducts(15, null, null, null);

        // Assert
        Assert.Single(result.Products);
        Assert.Equal("2", result.Products.First().title);
    }

    [Fact]
    public async Task GetFilteredProducts_WithHighlight_HighlightsWords()
    {
        // Arrange
        var testProducts = new[]
        {
            new Product { description = "A blue shirt" },
        };

        SetupMockHttpResponse(testProducts);

        // Act
        var result = await _productService.GetFilteredProducts(null, null, null, "blue");

        // Assert
        Assert.Contains("<em>blue</em>", result.Products.First().description);
    }

    private void SetupMockHttpResponse(Product[] products)
    {
        var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(products))
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }
}
