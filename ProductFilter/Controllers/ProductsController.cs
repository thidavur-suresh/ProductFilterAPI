using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ProductFilter.Core.Models;
using ProductFilter.Core.Services;

namespace ProductFilter.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Filters and retrieves products with optional highlighting
    /// </summary>
    /// <param name="minPrice">Minimum price filter</param>
    /// <param name="maxPrice">Maximum price filter</param>
    /// <param name="size">Size filter</param>
    /// <param name="highlight">Comma-separated words to highlight in descriptions</param>
    /// <returns>Filtered products with metadata</returns>
    /// <response code="200">Returns the filtered products</response>
    /// <response code="500">If there was an error processing the request</response>
    [HttpGet("filter")]
    [ProducesResponseType(typeof(FilteredResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]

    public async Task<IActionResult> Filter(
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? size,
        [FromQuery] string? highlight)
    {
        try
        {
            _logger.LogInformation(
                "Processing filter request: MinPrice={MinPrice}, MaxPrice={MaxPrice}, Size={Size}, Highlight={Highlight}",
                minPrice, maxPrice, size, highlight);

            var result = await _productService.GetFilteredProducts(minPrice, maxPrice, size, highlight);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing filter request");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }
}