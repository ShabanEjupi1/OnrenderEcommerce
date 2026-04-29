using System.ComponentModel.DataAnnotations;

namespace ProjectTemplate.Models.Ecommerce;

/// <summary>
/// Product category for the ecommerce store.
/// Mirrors KosovaPOS Kategoria / KategoriaPos tables.
/// </summary>
public class ProductCategory
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public int SortOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
