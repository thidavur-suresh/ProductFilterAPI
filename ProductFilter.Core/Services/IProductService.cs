using ProductFilter.Core.Models;

namespace ProductFilter.Core.Services;

public interface IProductService
{
    Task<FilteredResponse> GetFilteredProducts(
        decimal? minPrice,
        decimal? maxPrice,
        string? size,
        string? highlightWords);
}