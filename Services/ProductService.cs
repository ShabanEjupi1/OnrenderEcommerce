using Microsoft.EntityFrameworkCore;
using ProjectTemplate.Data;
using ProjectTemplate.Models.Ecommerce;

namespace ProjectTemplate.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _db;

    public ProductService(AppDbContext db) => _db = db;

    public async Task<(List<Product> Products, int Total)> GetProductsAsync(
        string? search, int? categoryId, string? sortBy, int page, int pageSize)
    {
        var query = _db.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            // Use a database-agnostic approach: translate to SQL LOWER() on the DB side.
            // We avoid calling .ToLower() on navigation properties as it can fail on
            // PostgreSQL when the related entity has a nullable FK. Instead we check the
            // denormalized CategoryName column first, and only touch Category.Name when
            // we know it is non-null via a left-join path that EF Core handles safely.
            var term = search.Trim().ToLower();
            query = query.Where(p =>
                EF.Functions.Like(p.Name.ToLower(), $"%{term}%") ||
                (p.Description != null && EF.Functions.Like(p.Description.ToLower(), $"%{term}%")) ||
                (p.Brand != null && EF.Functions.Like(p.Brand.ToLower(), $"%{term}%")) ||
                (p.CategoryName != null && EF.Functions.Like(p.CategoryName.ToLower(), $"%{term}%")));
        }

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        query = sortBy switch
        {
            "price_asc"  => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "name_asc"   => query.OrderBy(p => p.Name),
            "newest"     => query.OrderByDescending(p => p.CreatedAt),
            _            => query.OrderByDescending(p => p.IsFeatured).ThenBy(p => p.Name)
        };

        var total = await query.CountAsync();
        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (products, total);
    }

    public async Task<Product?> GetByIdAsync(int id) =>
        await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<List<Product>> GetFeaturedAsync(int count = 8) =>
        await _db.Products
            .Where(p => p.IsActive && p.IsFeatured)
            .OrderBy(p => p.Name)
            .Take(count)
            .ToListAsync();

    public async Task<List<Product>> GetMostSoldAsync(int count = 8) =>
        await _db.Products
            .Where(p => p.IsActive && p.IsMostSold)
            .OrderBy(p => p.Name)
            .Take(count)
            .ToListAsync();

    public async Task<List<Product>> GetRelatedAsync(int productId, int? categoryId, int count = 6) =>
        await _db.Products
            .Where(p => p.IsActive && p.Id != productId &&
                        (categoryId == null || p.CategoryId == categoryId))
            .OrderBy(_ => EF.Functions.Random())
            .Take(count)
            .ToListAsync();

    public async Task<List<ProductCategory>> GetCategoriesAsync() =>
        await _db.ProductCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();

    public async Task<Product> CreateAsync(Product product)
    {
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return product;
    }

    public async Task UpdateAsync(Product product)
    {
        product.UpdatedAt = DateTime.UtcNow;
        _db.Products.Update(product);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product != null)
        {
            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    public async Task<bool> SeedFromPosDataAsync(IEnumerable<PosImportItem> items)
    {
        try
        {
            // Ensure all categories exist
            var categoryNames = items
                .Where(i => !string.IsNullOrWhiteSpace(i.Category))
                .Select(i => i.Category!)
                .Distinct()
                .ToList();

            var existingCategories = await _db.ProductCategories.ToListAsync();
            foreach (var catName in categoryNames)
            {
                if (!existingCategories.Any(c => c.Name.ToLower() == catName.ToLower()))
                {
                    var cat = new ProductCategory { Name = catName, IsActive = true };
                    _db.ProductCategories.Add(cat);
                    existingCategories.Add(cat);
                }
            }
            await _db.SaveChangesAsync();

            // Reload categories with IDs
            existingCategories = await _db.ProductCategories.ToListAsync();

            foreach (var item in items)
            {
                var catId = item.Category != null
                    ? existingCategories
                        .FirstOrDefault(c => c.Name.ToLower() == item.Category.ToLower())?.Id
                    : null;

                // Update existing or insert new
                var existing = item.PosArticleId.HasValue
                    ? await _db.Products.FirstOrDefaultAsync(p => p.PosArticleId == item.PosArticleId)
                    : await _db.Products.FirstOrDefaultAsync(p => p.Barcode == item.Barcode);

                if (existing != null)
                {
                    existing.Name = item.Name;
                    existing.Price = item.Price;
                    existing.CategoryId = catId;
                    existing.CategoryName = item.Category;
                    existing.Brand = item.Brand;
                    existing.Unit = item.Unit;
                    existing.StockQuantity = item.StockQuantity;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    _db.Products.Add(new Product
                    {
                        Barcode = item.Barcode,
                        Name = item.Name,
                        Price = item.Price,
                        CategoryId = catId,
                        CategoryName = item.Category,
                        Brand = item.Brand,
                        Unit = item.Unit,
                        StockQuantity = item.StockQuantity,
                        PosArticleId = item.PosArticleId,
                        IsActive = true
                    });
                }
            }
            await _db.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
