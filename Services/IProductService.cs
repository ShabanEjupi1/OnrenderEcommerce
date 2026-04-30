using ProjectTemplate.Models.Ecommerce;

namespace ProjectTemplate.Services;

public interface IProductService
{
    Task<(List<Product> Products, int Total)> GetProductsAsync(
        string? search, int? categoryId, string? sortBy, int page, int pageSize);

    Task<Product?> GetByIdAsync(int id);
    Task<List<Product>> GetFeaturedAsync(int count = 8);
    Task<List<Product>> GetMostSoldAsync(int count = 8);
    Task<List<Product>> GetRelatedAsync(int productId, int? categoryId, int count = 6);
    Task<List<ProductCategory>> GetCategoriesAsync();

    // Admin operations
    Task<Product> CreateAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
    Task<bool> SeedFromPosDataAsync(IEnumerable<PosImportItem> items);
}

/// <summary>Data-transfer object for seeding products from POS exports.</summary>
public class PosImportItem
{
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public string? Unit { get; set; }
    public decimal StockQuantity { get; set; }
    public long? PosArticleId { get; set; }
}
