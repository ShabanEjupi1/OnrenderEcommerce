using System.ComponentModel.DataAnnotations;

namespace ProjectTemplate.Models.Ecommerce;

// ── View Models for Shop pages ────────────────────────────────────────────────

public class ShopIndexViewModel
{
    public List<Product> Products { get; set; } = new();
    public List<ProductCategory> Categories { get; set; } = new();
    public string? SelectedCategory { get; set; }
    public string? SearchQuery { get; set; }
    public int TotalProducts { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 24;
    public int TotalPages => (int)Math.Ceiling((double)TotalProducts / PageSize);
    public string? SortBy { get; set; }
}

public class ProductDetailViewModel
{
    public Product Product { get; set; } = null!;
    public List<Product> RelatedProducts { get; set; } = new();
}

// ── View Models for Cart ──────────────────────────────────────────────────────

public class CartViewModel
{
    public ShoppingCart Cart { get; set; } = new();
    public decimal DeliveryFee { get; set; } = 0;
    public decimal Total => Cart.SubTotal + DeliveryFee;
}

// ── View Models for Checkout ──────────────────────────────────────────────────

public class CheckoutViewModel
{
    [Required(ErrorMessage = "Emri është i detyrueshëm")]
    [StringLength(200)]
    public string CustomerName { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Email i pavlefshëm")]
    [StringLength(200)]
    public string? CustomerEmail { get; set; }

    [Required(ErrorMessage = "Numri i telefonit është i detyrueshëm")]
    [StringLength(50)]
    public string CustomerPhone { get; set; } = string.Empty;

    [StringLength(500)]
    public string? DeliveryAddress { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;

    // Read-only cart summary displayed on the page
    public ShoppingCart Cart { get; set; } = new();
}

public class OrderConfirmationViewModel
{
    public Order Order { get; set; } = null!;
}
