using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ProductFilter.Core.Models;
using System.Net.Http;
using Microsoft.AspNetCore.Http;

namespace ProductFilter.Core.Services;

public class ProductService : IProductService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ProductService> _logger;
    private const string ProductSourceUrl = "https://pastebin.com/raw/JucRNpWs";
    private const string PRIMARY_API_KEY = "0c4bbda1-bf7b-479d-b619-83a1df21f4e7";
    private const string SECONDARY_API_KEY = "a909ff08-d41b-4995-b2af-7b3efbdba597";
    private int errorcode = 0;

    public ProductService(IHttpClientFactory httpClientFactory, ILogger<ProductService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<FilteredResponse> GetFilteredProducts(
        decimal? minPrice,
        decimal? maxPrice,
        string? size,
        string? highlightWords)
    {
        var allProducts = await FetchProducts();
        _logger.LogInformation("Retrieved {Count} products from source", allProducts.Count());

        if (errorcode == StatusCodes.Status400BadRequest)
           _logger.LogError("Error Code {errorcode} Bad request. Invalid API Key", errorcode);
        else if (allProducts.Count() == 0)
           _logger.LogError("Error Code {errorcode} No data found", StatusCodes.Status204NoContent);



        var filteredProducts = FilterProducts(allProducts, minPrice, maxPrice, size);
        var highlightedProducts = HighlightWords(filteredProducts, highlightWords);

        return new FilteredResponse
        {
            Products = (List<Product>)highlightedProducts,
            Filter = GenerateMetadata(filteredProducts)
        };
    }

    private async Task<IEnumerable<Product>> FetchProducts()
    {
        try
        {
            using var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(ProductSourceUrl);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Raw API Response: {Response}", content);
            var allcontent = JsonSerializer.Deserialize<SourceData>(content);

            IEnumerable<Product> products = allcontent.products;

            if (String.Compare(allcontent.apiKeys.primary, PRIMARY_API_KEY) == 0 && String.Compare(allcontent.apiKeys.secondary, SECONDARY_API_KEY) == 0)
            {
               // products = allcontent.products;
                _logger.LogInformation("Retrieved {count} products from source ", allcontent.products.Count.ToString());
            }
            else
            {
                errorcode = StatusCodes.Status400BadRequest;
            }

            return (products)
                   ?? Enumerable.Empty<Product>();



        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching products from source");
            throw;
        }
    }

    private IEnumerable<Product> FilterProducts(
        IEnumerable<Product> products,
        decimal? minPrice,
        decimal? maxPrice,
        string? size)
    {
        var query = products.AsQueryable();

        if (minPrice.HasValue)
            query = query.Where(p => p.price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.price <= maxPrice.Value);

        if (!string.IsNullOrWhiteSpace(size))
        {
            foreach (string s in size.Split(','))
            {
                query = query.Where(p => p.sizes.Contains(s));
            }
        }

        return query.ToList();
    }

    private IEnumerable<Product> HighlightWords(
        IEnumerable<Product> products,
        string? highlightWords)
    {
        if (string.IsNullOrWhiteSpace(highlightWords))
            return products;

        var words = highlightWords.Split(',')
            .Select(w => w.Trim())
            .Where(w => !string.IsNullOrWhiteSpace(w))
            .ToArray();

        if (!words.Any())
            return products;

        var pattern = $@"\b({string.Join("|", words)})\b";
        var regex = new Regex(pattern, RegexOptions.IgnoreCase);

        return products.Select(p => new Product
        {
            title = p.title,
            price = p.price,
            sizes = p.sizes,
            description = regex.Replace(p.description, "<em>$1</em>")
        });
    }

    private FilterMetadata GenerateMetadata(IEnumerable<Product> products)
    {
        var commonWords = GetCommonWords(products)
            .Skip(5)  // Skip the 5 most common words
            .Take(10) // Take the next 10 most common words
            .ToList();

        return new FilterMetadata
        {
            MinPrice = products.Min(p => p.price),
            MaxPrice = products.Max(p => p.price),
            //Sizes = (List<string>)products.Select(p => p.sizes).Distinct(),
            CommonWords = commonWords.ToArray()
        };
    }

    private IEnumerable<string> GetCommonWords(IEnumerable<Product> products)
    {
        var wordPattern = @"\b\w+\b";
        var commonWords = products
            .SelectMany(p => Regex.Matches(p.description.ToLower(), wordPattern)
                .Select(m => m.Value))
            .GroupBy(word => word)
            .OrderByDescending(group => group.Count())
            .Select(group => group.Key);

        return commonWords;
    }
}