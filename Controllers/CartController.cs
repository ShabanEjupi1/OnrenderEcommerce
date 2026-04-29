using Microsoft.AspNetCore.Mvc;
using ProjectTemplate.Models.Ecommerce;
using ProjectTemplate.Services;

namespace ProjectTemplate.Controllers;

/// <summary>
/// Session-based shopping cart: add, remove, update quantity, view.
/// All mutating actions return JSON so they work via AJAX.
/// </summary>
public class CartController : Controller
{
    private readonly ICartService _cart;
    private readonly IProductService _products;

    public CartController(ICartService cart, IProductService products)
    {
        _cart = cart;
        _products = products;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var cart = _cart.GetCart(HttpContext);
        ViewData["CartCount"] = cart.TotalItems;
        return View(new CartViewModel { Cart = cart });
    }

    [HttpPost]
    public async Task<IActionResult> Add(int productId, decimal quantity = 1)
    {
        var product = await _products.GetByIdAsync(productId);
        if (product == null)
            return Json(new { success = false, message = "Produkti nuk u gjet." });

        if (!product.InStock)
            return Json(new { success = false, message = "Produkti nuk është në stok." });

        _cart.AddItem(HttpContext, new CartItem
        {
            ProductId   = product.Id,
            ProductName = product.Name,
            ImageUrl    = product.ImageUrl,
            UnitPrice   = product.DisplayPrice,
            Quantity    = quantity,
            Unit        = product.Unit
        });

        var updated = _cart.GetCart(HttpContext);
        return Json(new { success = true, cartCount = updated.TotalItems });
    }

    [HttpPost]
    public IActionResult Remove(int productId)
    {
        _cart.RemoveItem(HttpContext, productId);
        var updated = _cart.GetCart(HttpContext);
        return Json(new
        {
            success   = true,
            cartCount = updated.TotalItems,
            subTotal  = updated.SubTotal.ToString("F2")
        });
    }

    [HttpPost]
    public IActionResult UpdateQuantity(int productId, decimal quantity)
    {
        _cart.UpdateQuantity(HttpContext, productId, quantity);
        var updated = _cart.GetCart(HttpContext);
        var item = updated.Items.FirstOrDefault(i => i.ProductId == productId);
        return Json(new
        {
            success    = true,
            cartCount  = updated.TotalItems,
            subTotal   = updated.SubTotal.ToString("F2"),
            itemTotal  = item?.TotalPrice.ToString("F2") ?? "0.00"
        });
    }

    [HttpGet]
    public IActionResult Count()
    {
        var count = _cart.GetCart(HttpContext).TotalItems;
        return Json(new { count });
    }
}
