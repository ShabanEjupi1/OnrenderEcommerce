using Microsoft.AspNetCore.Mvc;
using ProjectTemplate.Models.Ecommerce;
using ProjectTemplate.Services;

namespace ProjectTemplate.Controllers;

/// <summary>
/// Handles the checkout flow: display form, process order, confirmation page.
/// </summary>
public class CheckoutController : Controller
{
    private readonly ICartService _cart;
    private readonly IOrderService _orders;

    public CheckoutController(ICartService cart, IOrderService orders)
    {
        _cart   = cart;
        _orders = orders;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var cart = _cart.GetCart(HttpContext);
        if (cart.Items.Count == 0)
            return RedirectToAction("Index", "Cart");

        ViewData["CartCount"] = cart.TotalItems;
        return View(new CheckoutViewModel { Cart = cart });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
    {
        var cart = _cart.GetCart(HttpContext);
        model.Cart = cart;

        if (cart.Items.Count == 0)
            return RedirectToAction("Index", "Cart");

        if (!ModelState.IsValid)
        {
            ViewData["CartCount"] = cart.TotalItems;
            return View("Index", model);
        }

        var order = await _orders.PlaceOrderAsync(model, cart);
        _cart.ClearCart(HttpContext);

        return RedirectToAction("Confirmation", new { orderNumber = order.OrderNumber });
    }

    [HttpGet]
    public async Task<IActionResult> Confirmation(string orderNumber)
    {
        var order = await _orders.GetByOrderNumberAsync(orderNumber);
        if (order == null) return NotFound();

        ViewData["CartCount"] = 0;
        return View(new OrderConfirmationViewModel { Order = order });
    }
}
