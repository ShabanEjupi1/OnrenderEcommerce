using Microsoft.AspNetCore.Mvc;
using ProjectTemplate.Models.Ecommerce;
using ProjectTemplate.Services;

namespace ProjectTemplate.Controllers;

/// <summary>
/// Handles the product catalog: listing, search, filtering and detail pages.
/// </summary>
public class ShopController : Controller
{
    private readonly IProductService _products;
    private readonly ICartService _cart;

    public ShopController(IProductService products, ICartService cart)
    {
        _products = products;
        _cart = cart;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? q, int? categoryId, string? sort, int page = 1)
    {
        const int pageSize = 24;
        var (products, total) = await _products.GetProductsAsync(q, categoryId, sort, page, pageSize);
        var categories = await _products.GetCategoriesAsync();

        var vm = new ShopIndexViewModel
        {
            Products         = products,
            Categories       = categories,
            SearchQuery      = q,
            SelectedCategory = categoryId?.ToString(),
            SortBy           = sort,
            Page             = page,
            PageSize         = pageSize,
            TotalProducts    = total
        };

        ViewData["CartCount"] = _cart.GetCart(HttpContext).TotalItems;
        return View(vm);
    }

    [HttpGet]
    [Route("shop/product/{id:int}/{slug?}")]
    public async Task<IActionResult> Detail(int id)
    {
        var product = await _products.GetByIdAsync(id);
        if (product == null) return NotFound();

        var related = await _products.GetRelatedAsync(id, product.CategoryId);

        ViewData["CartCount"] = _cart.GetCart(HttpContext).TotalItems;
        return View(new ProductDetailViewModel
        {
            Product        = product,
            RelatedProducts = related
        });
    }

    /// <summary>Returns featured products as JSON for the homepage.</summary>
    [HttpGet]
    public async Task<IActionResult> Featured()
    {
        var products = await _products.GetFeaturedAsync(8);
        return Json(MapToCards(products));
    }

    /// <summary>Returns newest products as JSON for the homepage.</summary>
    [HttpGet]
    public async Task<IActionResult> Newest()
    {
        var (products, _) = await _products.GetProductsAsync(null, null, "newest", 1, 8);
        return Json(MapToCards(products));
    }

    /// <summary>Returns product search suggestions as JSON for the autocomplete.</summary>
    [HttpGet]
    public async Task<IActionResult> Search(string q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Json(new List<object>());

        var (results, _) = await _products.GetProductsAsync(q, null, null, 1, 8);
        return Json(results.Select(p => new
        {
            id    = p.Id,
            name  = p.Name,
            price = p.DisplayPrice.ToString("F2"),
            image = p.ImageUrl ?? "/img/placeholder.png"
        }));
    }

    // -- Helpers ---------------------------------------------------------------

    private static object MapToCards(IEnumerable<ProjectTemplate.Models.Ecommerce.Product> products) =>
        products.Select(p => new
        {
            id              = p.Id,
            name            = p.Name,
            price           = p.Price.ToString("F2"),
            displayPrice    = p.DisplayPrice.ToString("F2"),
            isOnSale        = p.IsOnSale,
            discountPercent = p.DiscountPercent,
            categoryName    = p.CategoryName ?? p.Category?.Name,
            image           = p.ImageUrl ?? "/img/placeholder.png"
        });
}
