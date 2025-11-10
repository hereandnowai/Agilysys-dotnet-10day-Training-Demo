using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace First_Sample_Project_Prompting.Models;

/// <summary>
/// Entity model representing a customer in the POS system.
/// </summary>
[Table("POSCustomer")]
public class POSCustomer
{
    /// <summary>
    /// Gets or sets the unique identifier for the customer.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the customer's name.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's email address (must be unique).
    /// </summary>
    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's phone number (optional).
    /// </summary>
    [MaxLength(20)]
    public string? Phone { get; set; }

    /// <summary>
    /// Gets or sets the customer's billing address.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string billing_address { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the customer record was created.
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp when the customer record was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
