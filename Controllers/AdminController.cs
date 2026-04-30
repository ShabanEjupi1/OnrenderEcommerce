using Microsoft.AspNetCore.Mvc;
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

    public AdminController(IProductService products, IOrderService orders, IConfiguration config)
    {
        _products = products;
        _orders   = orders;
        _config   = config;
    }

    // -- Authentication --------------------------------------------------------

    private bool IsAuthenticated =>
        HttpContext.Session.GetString("AdminAuth") == "1";

    private IActionResult RequireAuth() =>
        IsAuthenticated ? null! : RedirectToAction("Login");

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
        if (auth != null!) return auth;

        var orders   = await _orders.GetAllOrdersAsync(1, 10);
        var cats     = await _products.GetCategoriesAsync();
        var (prods, total) = await _products.GetProductsAsync(null, null, null, 1, 5);
        ViewBag.RecentOrders = orders;
        ViewBag.Categories   = cats;
        ViewBag.TotalProducts = total;
        return View();
    }

    // -- Products --------------------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> Products(string? q, int? catId, int page = 1)
    {
        var auth = RequireAuth();
        if (auth != null!) return auth;

        var (products, total) = await _products.GetProductsAsync(q, catId, null, page, 50);
        var categories = await _products.GetCategoriesAsync();
        ViewBag.Products    = products;
        ViewBag.Categories  = categories;
        ViewBag.Total       = total;
        ViewBag.Page        = page;
        ViewBag.Query       = q;
        ViewBag.SelectedCat = catId;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> EditProduct(int id)
    {
        var auth = RequireAuth();
        if (auth != null!) return auth;

        var product    = await _products.GetByIdAsync(id);
        var categories = await _products.GetCategoriesAsync();
        ViewBag.Categories = categories;
        return View(product ?? new Product());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProduct(Product model)
    {
        var auth = RequireAuth();
        if (auth != null!) return auth;

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
        if (auth != null!) return auth;

        await _products.DeleteAsync(id);
        TempData["Success"] = "Produkti u fshi (deaktivizua).";
        return RedirectToAction("Products");
    }

    // -- Orders ----------------------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> Orders(int page = 1)
    {
        var auth = RequireAuth();
        if (auth != null!) return auth;

        var orders = await _orders.GetAllOrdersAsync(page, 50);
        ViewBag.Orders = orders;
        ViewBag.Page   = page;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus status)
    {
        var auth = RequireAuth();
        if (auth != null!) return auth;

        await _orders.UpdateStatusAsync(orderId, status);
        return Json(new { success = true });
    }

    // -- Product Import from POS CSV -------------------------------------------

    [HttpGet]
    public IActionResult Import()
    {
        var auth = RequireAuth();
        if (auth != null!) return auth;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Import(IFormFile csvFile)
    {
        var auth = RequireAuth();
        if (auth != null!) return auth;

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
                    Barcode       = cols[0].Trim(),
                    Name          = cols[1].Trim(),
                    Price         = price,
                    Category      = cols.Length > 3 ? cols[3].Trim() : null,
                    Brand         = cols.Length > 4 ? cols[4].Trim() : null,
                    Unit          = cols.Length > 5 ? cols[5].Trim() : null,
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
    // -- Profiles, Messages, Settings (Mock) -----------------------------------

    [HttpGet]
    public IActionResult Profiles()
    {
        var auth = RequireAuth();
        if (auth != null!) return auth;
        return View();
    }

    [HttpGet]
    public IActionResult Messages()
    {
        var auth = RequireAuth();
        if (auth != null!) return auth;
        return View();
    }

    [HttpGet]
    public IActionResult Settings()
    {
        var auth = RequireAuth();
        if (auth != null!) return auth;
        return View();
    }
}
