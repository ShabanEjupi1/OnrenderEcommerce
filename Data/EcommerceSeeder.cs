using ProjectTemplate.Data;
using ProjectTemplate.Models.Ecommerce;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ProjectTemplate.Data;

/// <summary>
/// Seeds the ecommerce catalog with demo products or syncs from the Enisi Center POS.
/// </summary>
public static class EcommerceSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Products.Any()) return; // Already seeded

        bool isSynced = TrySyncFromPOS(db);
        if (isSynced)
        {
            Console.WriteLine("[EcommerceSeeder] Successfully synced from POS Database.");
            return;
        }

        // ── Categories ─────────────────────────────────────────────────────────
        var categories = new[]
        {
            new ProductCategory { Name = "Elektroshtepiake",    SortOrder = 1, IsActive = true },
            new ProductCategory { Name = "Ushqim & Pije",       SortOrder = 2, IsActive = true },
            new ProductCategory { Name = "Veshmbathje",         SortOrder = 3, IsActive = true },
            new ProductCategory { Name = "Kozmetikë",           SortOrder = 4, IsActive = true },
            new ProductCategory { Name = "Elektronikë",         SortOrder = 5, IsActive = true },
            new ProductCategory { Name = "Shtëpi & Kuzhinë",    SortOrder = 6, IsActive = true },
            new ProductCategory { Name = "Sport & Aktivitet",   SortOrder = 7, IsActive = true },
            new ProductCategory { Name = "Lodra & Fëmijë",      SortOrder = 8, IsActive = true },
        };
        db.ProductCategories.AddRange(categories);
        db.SaveChanges();

        var catMap = db.ProductCategories.ToDictionary(c => c.Name);

        // ── Products ───────────────────────────────────────────────────────────
        var products = new[]
        {
            // Elektroshtepiake
            new Product { Name = "Frigorifer Samsung 350L", Price = 459.00m, StockQuantity = 15, CategoryId = catMap["Elektroshtepiake"].Id, CategoryName = "Elektroshtepiake", Brand = "Samsung", Unit = "Copë", IsActive = true, IsFeatured = true, Description = "Frigorifer me dy dyer, kapacitet 350 litra, klasa energjetike A++." },
            new Product { Name = "Lavatriçe Bosch 7kg",    Price = 389.00m, StockQuantity = 8,  CategoryId = catMap["Elektroshtepiake"].Id, CategoryName = "Elektroshtepiake", Brand = "Bosch",   Unit = "Copë", IsActive = true, IsFeatured = true, Description = "Lavatriçe me kapacitet 7kg, 1200 rrotullime, programim elektronik." },
            new Product { Name = "Mikrovalë LG 25L",        Price = 89.00m,  StockQuantity = 20, CategoryId = catMap["Elektroshtepiake"].Id, CategoryName = "Elektroshtepiake", Brand = "LG",     Unit = "Copë", IsActive = true },
            new Product { Name = "Aspirator Philips",       Price = 65.00m,  StockQuantity = 30, CategoryId = catMap["Elektroshtepiake"].Id, CategoryName = "Elektroshtepiake", Brand = "Philips", Unit = "Copë", IsActive = true },

            // Elektronikë
            new Product { Name = "Laptop Lenovo IdeaPad 15\"",  Price = 649.00m, StockQuantity = 6,  CategoryId = catMap["Elektronikë"].Id, CategoryName = "Elektronikë", Brand = "Lenovo", Unit = "Copë", IsActive = true, IsFeatured = true, Description = "Procesori Intel Core i5, RAM 8GB, SSD 512GB, ekran 15.6\" Full HD." },
            new Product { Name = "Smartphone Samsung A55",       Price = 329.00m, StockQuantity = 12, CategoryId = catMap["Elektronikë"].Id, CategoryName = "Elektronikë", Brand = "Samsung", Unit = "Copë", IsActive = true, IsFeatured = true },
            new Product { Name = "Kufje Sony WH-1000XM5",       Price = 249.00m, SalePrice = 199.00m, StockQuantity = 5, CategoryId = catMap["Elektronikë"].Id, CategoryName = "Elektronikë", Brand = "Sony", Unit = "Copë", IsActive = true },
            new Product { Name = "Tablet Apple iPad 10.9\"",    Price = 499.00m, StockQuantity = 4,  CategoryId = catMap["Elektronikë"].Id, CategoryName = "Elektronikë", Brand = "Apple", Unit = "Copë", IsActive = true },

            // Ushqim & Pije
            new Product { Name = "Kafe Lavazza Oro 1kg",        Price = 12.50m, StockQuantity = 100, CategoryId = catMap["Ushqim & Pije"].Id, CategoryName = "Ushqim & Pije", Brand = "Lavazza", Unit = "KG",  IsActive = true, IsFeatured = true },
            new Product { Name = "Ujë Rugove 1.5L (kuti 6)",    Price = 3.20m,  StockQuantity = 500, CategoryId = catMap["Ushqim & Pije"].Id, CategoryName = "Ushqim & Pije", Unit = "Kuti",     IsActive = true },
            new Product { Name = "Mjaltë natyral 500g",         Price = 8.90m,  StockQuantity = 80,  CategoryId = catMap["Ushqim & Pije"].Id, CategoryName = "Ushqim & Pije", Unit = "Copë",     IsActive = true },
            new Product { Name = "Çaj Twinings English Breakfast", Price = 4.50m, StockQuantity = 120, CategoryId = catMap["Ushqim & Pije"].Id, CategoryName = "Ushqim & Pije", Brand = "Twinings", Unit = "Copë", IsActive = true },

            // Veshmbathje
            new Product { Name = "Këpucë sportive Nike Air Max", Price = 129.00m, SalePrice = 99.00m, StockQuantity = 25, CategoryId = catMap["Veshmbathje"].Id, CategoryName = "Veshmbathje", Brand = "Nike", Unit = "Palë", IsActive = true, IsFeatured = true },
            new Product { Name = "Xhaketë dimri Adidas",         Price = 85.00m,  StockQuantity = 18, CategoryId = catMap["Veshmbathje"].Id, CategoryName = "Veshmbathje", Brand = "Adidas", Unit = "Copë", IsActive = true },
            new Product { Name = "Çantë shpine Samsonite",        Price = 59.00m,  StockQuantity = 14, CategoryId = catMap["Veshmbathje"].Id, CategoryName = "Veshmbathje", Brand = "Samsonite", Unit = "Copë", IsActive = true },

            // Kozmetikë
            new Product { Name = "Parfum Chanel No.5 100ml",     Price = 89.00m,  StockQuantity = 10, CategoryId = catMap["Kozmetikë"].Id, CategoryName = "Kozmetikë", Brand = "Chanel", Unit = "Copë", IsActive = true, IsFeatured = true },
            new Product { Name = "Krem fytyre L'Oreal Hydra",     Price = 15.90m, StockQuantity = 50, CategoryId = catMap["Kozmetikë"].Id, CategoryName = "Kozmetikë", Brand = "L'Oreal", Unit = "Copë", IsActive = true },
            new Product { Name = "Shampo Pantene Pro-V 400ml",    Price = 5.90m,  StockQuantity = 80, CategoryId = catMap["Kozmetikë"].Id, CategoryName = "Kozmetikë", Brand = "Pantene", Unit = "Copë", IsActive = true },

            // Shtëpi & Kuzhinë
            new Product { Name = "Set gatimi 10 copë inox",       Price = 79.00m, StockQuantity = 12, CategoryId = catMap["Shtëpi & Kuzhinë"].Id, CategoryName = "Shtëpi & Kuzhinë", Unit = "Set", IsActive = true },
            new Product { Name = "Kafetierë De'Longhi Automatik", Price = 299.00m, SalePrice = 249.00m, StockQuantity = 7, CategoryId = catMap["Shtëpi & Kuzhinë"].Id, CategoryName = "Shtëpi & Kuzhinë", Brand = "De'Longhi", Unit = "Copë", IsActive = true, IsFeatured = true },
            new Product { Name = "Pjata porcelani 6 copë",        Price = 29.00m, StockQuantity = 30, CategoryId = catMap["Shtëpi & Kuzhinë"].Id, CategoryName = "Shtëpi & Kuzhinë", Unit = "Set", IsActive = true },

            // Sport
            new Product { Name = "Biçikletë mountain bike 26\"", Price = 249.00m, StockQuantity = 5, CategoryId = catMap["Sport & Aktivitet"].Id, CategoryName = "Sport & Aktivitet", Unit = "Copë", IsActive = true },
            new Product { Name = "Topin futbolli Adidas",         Price = 29.00m,  StockQuantity = 40, CategoryId = catMap["Sport & Aktivitet"].Id, CategoryName = "Sport & Aktivitet", Brand = "Adidas", Unit = "Copë", IsActive = true },

            // Lodra
            new Product { Name = "LEGO City Set 500 copa",       Price = 49.00m, StockQuantity = 15, CategoryId = catMap["Lodra & Fëmijë"].Id, CategoryName = "Lodra & Fëmijë", Brand = "LEGO", Unit = "Copë", IsActive = true },
            new Product { Name = "Kukullë Barbie Dreamhouse",    Price = 89.00m, StockQuantity = 8,  CategoryId = catMap["Lodra & Fëmijë"].Id, CategoryName = "Lodra & Fëmijë", Brand = "Barbie", Unit = "Copë", IsActive = true },
        };

        db.Products.AddRange(products);
        db.SaveChanges();

        Console.WriteLine($"[EcommerceSeeder] Seeded {categories.Length} categories and {products.Length} products.");
    }

    private static bool TrySyncFromPOS(AppDbContext db)
    {
        string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=BMDData;Trusted_Connection=True;TrustServerCertificate=True;Connection Timeout=5;";
        try
        {
            using var conn = new SqlConnection(connectionString);
            conn.Open();

            // Check if Artikujt exists
            using var cmd = new SqlCommand("SELECT id, Barkodi, Emertimi, CShitjes, Sasia, Kategoria, Prodhuesi, NjesiaP, Tatimi FROM Artikujt", conn);
            using var reader = cmd.ExecuteReader();

            var categories = new Dictionary<string, ProductCategory>();
            var products = new List<Product>();

            while (reader.Read())
            {
                var categoryName = reader.IsDBNull(5) ? "Te Tjera" : reader.GetString(5);
                if (string.IsNullOrWhiteSpace(categoryName)) categoryName = "Te Tjera";

                if (!categories.TryGetValue(categoryName, out var category))
                {
                    category = new ProductCategory { Name = categoryName, IsActive = true };
                    categories[categoryName] = category;
                }

                var priceVal = reader.IsDBNull(3) ? 0.0 : reader.GetDouble(3);
                var stockVal = reader.IsDBNull(4) ? 0.0 : reader.GetDouble(4);

                products.Add(new Product
                {
                    PosArticleId = reader.GetInt64(0),
                    Barcode = reader.IsDBNull(1) ? null : reader.GetString(1),
                    Name = reader.IsDBNull(2) ? "Unknown" : reader.GetString(2),
                    Price = (decimal)priceVal,
                    StockQuantity = (decimal)stockVal,
                    CategoryName = categoryName,
                    Brand = reader.IsDBNull(6) ? null : reader.GetString(6),
                    Unit = reader.IsDBNull(7) ? null : reader.GetString(7),
                    VatRate = reader.IsDBNull(8) ? 18m : (decimal)reader.GetDouble(8),
                    IsActive = true,
                    IsFeatured = stockVal > 10,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            if (products.Count > 0)
            {
                var catList = categories.Values.ToList();
                db.ProductCategories.AddRange(catList);
                db.SaveChanges();

                var catMap = db.ProductCategories.ToDictionary(c => c.Name);
                foreach (var p in products)
                {
                    if (catMap.TryGetValue(p.CategoryName!, out var mappedCat))
                    {
                        p.CategoryId = mappedCat.Id;
                    }
                }

                db.Products.AddRange(products);
                db.SaveChanges();
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EcommerceSeeder] POS Sync failed: {ex.Message}");
        }
        
        return false;
    }
}
