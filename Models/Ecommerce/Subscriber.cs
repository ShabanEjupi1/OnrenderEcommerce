using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectTemplate.Models.Ecommerce;

/// <summary>
/// Represents a newsletter subscriber. Subscribers opt-in via the storefront footer
/// and can receive promotional campaigns sent from the admin panel.
/// </summary>
public class Subscriber
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Name { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UnsubscribedAt { get; set; }

    /// <summary>Unsubscribe token — sent in email links so users can opt-out without a login.</summary>
    [StringLength(64)]
    public string UnsubscribeToken { get; set; } = Guid.NewGuid().ToString("N");
}
