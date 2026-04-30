using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectTemplate.Data;
using ProjectTemplate.Models.Ecommerce;
using ProjectTemplate.Services;

namespace ProjectTemplate.Controllers;

/// <summary>
/// Admin panel for Enisi Center — protected by a simple password stored in environment variables.
/// Access: /Admin with the APP_PASSWORD cookie set.
/// </summary>
public class AdminController : Controller
{
    private readonly IProductService _products;
    private readonly IOrderService _orders;
    private readonly IConfiguration _config;
    private readonly AppDbContext _db;
    private readonly IEmailService _email;

    public AdminController(
        IProductService products,
        IOrderService orders,
        IConfiguration config,
        AppDbContext db,
        IEmailService email)
    {
        _products = products;
        _orders = orders;
        _config = config;
        _db = db;
        _email = email;
    }

    // -- Authentication --------------------------------------------------------

    private bool IsAuthenticated =>
        HttpContext.Session.GetString("AdminAuth") == "1";

    private IActionResult? RequireAuth() =>
        IsAuthenticated ? null : RedirectToAction("Login");

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public IActionResult Login(string password)
    {
        var expected = _config["APP_PASSWORD"] ?? Environment.GetEnvironmentVariable("APP_PASSWORD") ?? "admin123";
        if (password == expected)
        {
            HttpContext.Session.SetString("AdminAuth", "1");
            return RedirectToAction("Dashboard");
        }
        ModelState.AddModelError("", "Fjalëkalimi i gabuar.");
        return View();
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Remove("AdminAuth");
        return RedirectToAction("Login");
    }

    // -- Dashboard -------------------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var auth = RequireAuth();
        if (auth != null) return auth;

        var orders = await _orders.GetAllOrdersAsync(1, 10);
        var cats = await _products.GetCategoriesAsync();
        var (prods, total) = await _products.GetProductsAsync(null, null, null, 1, 5);

        var subscriberCount = await _db.Subscribers.CountAsync(s => s.IsActive);
        var featuredCount = await _db.Products.CountAsync(p => p.IsActive && p.IsFeatured);
        var onSaleCount = await _db.Products.CountAsync(p => p.IsActive && p.SalePrice != null && p.SalePrice < p.Price);
        var mostSoldCount = await _db.Products.CountAsync(p => p.IsActive && p.IsMostSold);

        ViewBag.RecentOrders = orders;
        ViewBag.Categories = cats;
        ViewBag.TotalProducts = total;
        ViewBag.SubscriberCount = subscriberCount;
        ViewBag.FeaturedCount = featuredCount;
        ViewBag.OnSaleCount = onSaleCount;
        ViewBag.MostSoldCount = mostSoldCount;
        return View();
    }

    // -- Products --------------------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> Products(string? q, int? catId, int page = 1)
    {
        var auth = RequireAuth();
        if (auth != null) return auth;

        var (products, total) = await _products.GetProductsAsync(q, catId, null, page, 50);
        var categories = await _products.GetCategoriesAsync();
        ViewBag.Products = products;
        ViewBag.Categories = categories;
        ViewBag.Total = total;
        ViewBag.Page = page;
        ViewBag.Query = q;
        ViewBag.SelectedCat = catId;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> EditProduct(int id)
    {
        var auth = RequireAuth();
        if (auth != null) return auth;

        var product = await _products.GetByIdAsync(id);
        var categories = await _products.GetCategoriesAsync();
        ViewBag.Categories = categories;
        return View(product ?? new Product());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProduct(Product model)
    {
        var auth = RequireAuth();
        if (auth != null) return auth;

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = await _products.GetCategoriesAsync();
            return View(model);
        }

        if (model.Id == 0)
            await _products.CreateAsync(model);
        else
            await _products.UpdateAsync(model);

        TempData["Success"] = model.Id == 0
            ? "Produkti u shtua me sukses!"
            : "Produkti u përditësua!";
        return RedirectToAction("Products");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var auth = RequireAuth();
        if (auth != null) return auth;

        await _products.DeleteAsync(id);
        TempData["Success"] = "Produkti u fshi (deaktivizua).";
        return RedirectToAction("Products");
    }

    // -- Promotions ------------------------------------------------------------

    /// <summary>
    /// Promotions hub — set sale prices, toggle featured/most-sold flags, send newsletter blasts.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Promotions(string? q, int? catId, int page = 1)
    {
        var auth = RequireAuth();
        if (auth != null) return auth;

        var (products, total) = await _products.GetProductsAsync(q, catId, null, page, 100);
        var categories = await _products.GetCategoriesAsync();
        var subscriberCount = await _db.Subscribers.CountAsync(s => s.IsActive);

        ViewBag.Products = products;
        ViewBag.Categories = categories;
        ViewBag.Total = total;
        ViewBag.Page = page;
        ViewBag.Query = q;
        ViewBag.SelectedCat = catId;
        ViewBag.SubscriberCount = subscriberCount;
        ViewBag.EmailConfigured = _email.IsConfigured;
        return View();
    }

    /// <summary>
    /// Quick-toggle IsFeatured flag via AJAX.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ToggleFeatured(int id)
    {
        var auth = RequireAuth();
        if (auth != null) return Json(new { success = false, error = "Unauthorized" });

        var product = await _db.Products.FindAsync(id);
        if (product == null) return Json(new { success = false, error = "Not found" });

        product.IsFeatured = !product.IsFeatured;
        product.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Json(new { success = true, isFeatured = product.IsFeatured });
    }

    /// <summary>
    /// Quick-toggle IsMostSold flag via AJAX.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ToggleMostSold(int id)
    {
        var auth = RequireAuth();
        if (auth != null) return Json(new { success = false, error = "Unauthorized" });

        var product = await _db.Products.FindAsync(id);
        if (product == null) return Json(new { success = false, error = "Not found" });

        product.IsMostSold = !product.IsMostSold;
        product.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Json(new { success = true, isMostSold = product.IsMostSold });
    }

    /// <summary>
    /// Apply or clear sale price via AJAX.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SetSalePrice(int id, decimal? salePrice)
    {
        var auth = RequireAuth();
        if (auth != null) return Json(new { success = false, error = "Unauthorized" });

        var product = await _db.Products.FindAsync(id);
        if (product == null) return Json(new { success = false, error = "Not found" });

        if (salePrice.HasValue && salePrice.Value > 0 && salePrice.Value < product.Price)
        {
            product.SalePrice = salePrice.Value;
            var discount = (int)Math.Round((product.Price - salePrice.Value) / product.Price * 100);
            product.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Json(new { success = true, salePrice = product.SalePrice, discountPercent = discount });
        }
        else if (salePrice == null || salePrice == 0)
        {
            product.SalePrice = null;
            product.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Json(new { success = true, salePrice = (decimal?)null, discountPercent = 0 });
        }

        return Json(new { success = false, error = "Çmimi i zbritur duhet të jetë më i vogël se çmimi origjinal." });
    }

    /// <summary>
    /// Bulk-apply a percentage discount to all selected products.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkDiscount(int[] productIds, int discountPercent)
    {
        var auth = RequireAuth();
        if (auth != null) return Json(new { success = false });

        if (discountPercent < 1 || discountPercent > 90 || productIds.Length == 0)
        {
            TempData["Error"] = "Parametra të pavlefshëm.";
            return RedirectToAction("Promotions");
        }

        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id) && p.IsActive)
            .ToListAsync();

        foreach (var p in products)
        {
            p.SalePrice = Math.Round(p.Price * (1 - discountPercent / 100m), 2);
            p.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Zbritja {discountPercent}% u aplikua për {products.Count} produkte.";
        return RedirectToAction("Promotions");
    }

    /// <summary>
    /// Clear all active sale prices.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearAllSalePrices()
    {
        var auth = RequireAuth();
        if (auth != null) return Json(new { success = false });

        var products = await _db.Products.Where(p => p.SalePrice != null).ToListAsync();
        foreach (var p in products)
        {
            p.SalePrice = null;
            p.UpdatedAt = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Të gjitha zbritjet u hoqën ({products.Count} produkte).";
        return RedirectToAction("Promotions");
    }

    // -- Newsletter blast -------------------------------------------------------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendNewsletter(string subject, string htmlBody)
    {
        var auth = RequireAuth();
        if (auth != null) return Json(new { success = false });

        if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(htmlBody))
        {
            TempData["Error"] = "Lënda dhe përmbajtja janë të detyrueshme.";
            return RedirectToAction("Promotions");
        }

        var subscribers = await _db.Subscribers
            .Where(s => s.IsActive)
            .Select(s => new { s.Email, s.UnsubscribeToken })
            .ToListAsync();

        if (subscribers.Count == 0)
        {
            TempData["Error"] = "Nuk ka abonentë aktivë.";
            return RedirectToAction("Promotions");
        }

        var recipients = subscribers.Select(s => (s.Email, s.UnsubscribeToken)).ToList();
        var ok = await _email.SendNewsletterAsync(recipients, subject, htmlBody);

        TempData[ok ? "Success" : "Error"] = ok
            ? $"Buletini u dërgua tek {subscribers.Count} abonentë!"
            : "Ndodhi një gabim gjatë dërgimit. Kontrolloni konfgurimin SMTP.";

        return RedirectToAction("Promotions");
    }

    // -- Subscribers -----------------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> Subscribers(int page = 1)
    {
        var auth = RequireAuth();
        if (auth != null) return auth;

        const int pageSize = 50;
        var query = _db.Subscribers.OrderByDescending(s => s.SubscribedAt);
        var total = await query.CountAsync();
        var subscribers = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Subscribers = subscribers;
        ViewBag.Total = total;
        ViewBag.Page = page;
        ViewBag.ActiveCount = await _db.Subscribers.CountAsync(s => s.IsActive);
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> DeleteSubscriber(int id)
    {
        var auth = RequireAuth();
        if (auth != null) return Json(new { success = false });

        var sub = await _db.Subscribers.FindAsync(id);
        if (sub != null)
        {
            _db.Subscribers.Remove(sub);
            await _db.SaveChangesAsync();
        }
        return Json(new { success = true });
    }

    // -- Orders ----------------------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> Orders(int page = 1)
    {
        var auth = RequireAuth();
        if (auth != null) return auth;

        var orders = await _orders.GetAllOrdersAsync(page, 50);
        ViewBag.Orders = orders;
        ViewBag.Page = page;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus status)
    {
        var auth = RequireAuth();
        if (auth != null) return Json(new { success = false });

        await _orders.UpdateStatusAsync(orderId, status);
        return Json(new { success = true });
    }

    // -- Product Import from POS CSV -------------------------------------------

    [HttpGet]
    public IActionResult Import()
    {
        var auth = RequireAuth();
        if (auth != null) return auth;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Import(IFormFile csvFile)
    {
        var auth = RequireAuth();
        if (auth != null) return auth;

        if (csvFile == null || csvFile.Length == 0)
        {
            ModelState.AddModelError("", "Ju lutem zgjidhni një skedar CSV.");
            return View();
        }

        var items = new List<PosImportItem>();
        using var reader = new System.IO.StreamReader(csvFile.OpenReadStream());
        string? header = await reader.ReadLineAsync(); // skip header

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;
            var cols = line.Split(',');
            if (cols.Length < 3) continue;

            if (decimal.TryParse(cols[2].Trim(), System.Globalization.NumberStyles.Any,
                                 System.Globalization.CultureInfo.InvariantCulture, out var price))
            {
                items.Add(new PosImportItem
                {
                    Barcode = cols[0].Trim(),
                    Name = cols[1].Trim(),
                    Price = price,
                    Category = cols.Length > 3 ? cols[3].Trim() : null,
                    Brand = cols.Length > 4 ? cols[4].Trim() : null,
                    Unit = cols.Length > 5 ? cols[5].Trim() : null,
                    StockQuantity = cols.Length > 6 && decimal.TryParse(cols[6].Trim(),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out var qty) ? qty : 0
                });
            }
        }

        var success = await _products.SeedFromPosDataAsync(items);
        TempData[success ? "Success" : "Error"] = success
            ? $"{items.Count} produkte u importuan me sukses!"
            : "Gabim gjatë importit. Kontrolloni skedarin.";

        return RedirectToAction("Products");
    }

    // -- Profiles, Messages, Settings ------------------------------------------

    [HttpGet]
    public IActionResult Profiles()
    {
        var auth = RequireAuth();
        if (auth != null) return auth;
        return View();
    }

    [HttpGet]
    public IActionResult Messages()
    {
        var auth = RequireAuth();
        if (auth != null) return auth;
        return View();
    }

    [HttpGet]
    public IActionResult Settings()
    {
        var auth = RequireAuth();
        if (auth != null) return auth;
        ViewBag.EmailConfigured = _email.IsConfigured;
        return View();
    }
}
