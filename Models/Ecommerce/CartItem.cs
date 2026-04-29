using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectTemplate.Models.Ecommerce;

/// <summary>
/// A single item in the in-memory / session-based shopping cart.
/// The cart itself is stored as JSON in the session — no DB table needed.
/// </summary>
public class CartItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Quantity { get; set; } = 1;
    public string? Unit { get; set; }

    [NotMapped]
    public decimal TotalPrice => UnitPrice * Quantity;
}

/// <summary>
/// The shopping cart stored in ASP.NET session.
/// </summary>
public class ShoppingCart
{
    public List<CartItem> Items { get; set; } = new();

    [NotMapped]
    public decimal SubTotal => Items.Sum(i => i.TotalPrice);

    [NotMapped]
    public int TotalItems => (int)Items.Sum(i => i.Quantity);

    public void AddItem(CartItem newItem)
    {
        var existing = Items.FirstOrDefault(i => i.ProductId == newItem.ProductId);
        if (existing != null)
            existing.Quantity += newItem.Quantity;
        else
            Items.Add(newItem);
    }

    public void RemoveItem(int productId)
    {
        Items.RemoveAll(i => i.ProductId == productId);
    }

    public void UpdateQuantity(int productId, decimal quantity)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            if (quantity <= 0)
                Items.Remove(item);
            else
                item.Quantity = quantity;
        }
    }

    public void Clear() => Items.Clear();
}
