using Newtonsoft.Json;
using ProjectTemplate.Models.Ecommerce;

namespace ProjectTemplate.Services;

public interface ICartService
{
    ShoppingCart GetCart(HttpContext context);
    void SaveCart(HttpContext context, ShoppingCart cart);
    void AddItem(HttpContext context, CartItem item);
    void RemoveItem(HttpContext context, int productId);
    void UpdateQuantity(HttpContext context, int productId, decimal quantity);
    void ClearCart(HttpContext context);
}

public class CartService : ICartService
{
    private const string CartSessionKey = "EnisiCart";

    public ShoppingCart GetCart(HttpContext context)
    {
        var json = context.Session.GetString(CartSessionKey);
        if (string.IsNullOrEmpty(json))
            return new ShoppingCart();
        return JsonConvert.DeserializeObject<ShoppingCart>(json) ?? new ShoppingCart();
    }

    public void SaveCart(HttpContext context, ShoppingCart cart) =>
        context.Session.SetString(CartSessionKey, JsonConvert.SerializeObject(cart));

    public void AddItem(HttpContext context, CartItem item)
    {
        var cart = GetCart(context);
        cart.AddItem(item);
        SaveCart(context, cart);
    }

    public void RemoveItem(HttpContext context, int productId)
    {
        var cart = GetCart(context);
        cart.RemoveItem(productId);
        SaveCart(context, cart);
    }

    public void UpdateQuantity(HttpContext context, int productId, decimal quantity)
    {
        var cart = GetCart(context);
        cart.UpdateQuantity(productId, quantity);
        SaveCart(context, cart);
    }

    public void ClearCart(HttpContext context)
    {
        context.Session.Remove(CartSessionKey);
    }
}
