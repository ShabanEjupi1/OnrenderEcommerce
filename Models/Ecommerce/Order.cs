using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectTemplate.Models.Ecommerce;

public enum OrderStatus
{
    Pending,
    Confirmed,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}

public enum PaymentMethod
{
    CashOnDelivery,
    BankTransfer,
    Card
}

/// <summary>
/// Represents a customer order placed through the Enisi Center webstore.
/// </summary>
public class Order
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    public string OrderNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string CustomerName { get; set; } = string.Empty;

    [StringLength(200)]
    [EmailAddress]
    public string? CustomerEmail { get; set; }

    [StringLength(50)]
    public string? CustomerPhone { get; set; }

    [StringLength(500)]
    public string? DeliveryAddress { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;

    [Column(TypeName = "decimal(10,2)")]
    public decimal SubTotal { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal DeliveryFee { get; set; } = 0;

    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    // -- Helpers --------------------------------------------------------------

    [NotMapped]
    public string StatusLabel => Status switch
    {
        OrderStatus.Pending     => "Në pritje",
        OrderStatus.Confirmed   => "Konfirmuar",
        OrderStatus.Processing  => "Në procesim",
        OrderStatus.Shipped     => "Dërguar",
        OrderStatus.Delivered   => "Dorëzuar",
        OrderStatus.Cancelled   => "Anuluar",
        _                       => Status.ToString()
    };
}

/// <summary>Line item within an order. Captures price at time of purchase.</summary>
public class OrderItem
{
    [Key]
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int ProductId { get; set; }

    [Required]
    [StringLength(250)]
    public string ProductName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? ProductImageUrl { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Quantity { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalPrice { get; set; }
}
