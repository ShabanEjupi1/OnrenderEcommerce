using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectTemplate.Models.Ecommerce;

/// <summary>
/// Ecommerce product model — mirrors the KosovaPOS BMDData.Artikujt table.
/// Products are managed in the POS and exposed via the web store.
/// </summary>
public class Product
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string? Barcode { get; set; }

    [Required]
    [StringLength(250)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    /// <summary>The selling price shown to customers.</summary>
    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    /// <summary>Optional promotional/sale price. Null means no active discount.</summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal? SalePrice { get; set; }

    public int? CategoryId { get; set; }
    public ProductCategory? Category { get; set; }

    [StringLength(150)]
    public string? CategoryName { get; set; }

    [StringLength(100)]
    public string? Brand { get; set; }

    [StringLength(20)]
    public string? Unit { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal StockQuantity { get; set; } = 0;

    /// <summary>URL or relative path to the product image.</summary>
    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsFeatured { get; set; } = false;

    /// <summary>VAT rate percentage (e.g. 18, 8, or 0).</summary>
    public decimal VatRate { get; set; } = 18;

    /// <summary>The original POS article ID for traceability.</summary>
    public long? PosArticleId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // -- Derived helpers ------------------------------------------------------

    [NotMapped]
    public decimal DisplayPrice => SalePrice.HasValue && SalePrice.Value < Price ? SalePrice.Value : Price;

    [NotMapped]
    public bool IsOnSale => SalePrice.HasValue && SalePrice.Value < Price && SalePrice.Value > 0;

    [NotMapped]
    public int DiscountPercent => IsOnSale
        ? (int)Math.Round((Price - SalePrice!.Value) / Price * 100)
        : 0;

    [NotMapped]
    public bool InStock => true;
}
