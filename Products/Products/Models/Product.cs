using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Products.Models;

/// <summary>
/// Represents a product entity in the system.
/// </summary>
public class Product
{
    /// <summary>
    /// Gets or sets the unique identifier for the product.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the name of the product.
    /// </summary>
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters")]
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the product.
    /// </summary>
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the unit price of the product.
    /// </summary>
    [Required(ErrorMessage = "Unit price is required")]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0")]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Gets or sets the SKU (Stock Keeping Unit) of the product.
    /// </summary>
    [Required(ErrorMessage = "SKU is required")]
    [StringLength(50, ErrorMessage = "SKU cannot exceed 50 characters")]
    public string SKU { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the available quantity of the product.
    /// </summary>
    [Required(ErrorMessage = "Quantity is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative")]
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the product was created.
    /// </summary>
    [Required]
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the user who created the product.
    /// </summary>
    [Required(ErrorMessage = "Created by is required")]
    [StringLength(100, ErrorMessage = "Created by cannot exceed 100 characters")]
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the product was last modified.
    /// </summary>
    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Gets or sets the user who last modified the product.
    /// </summary>
    [StringLength(100, ErrorMessage = "Modified by cannot exceed 100 characters")]
    public string? ModifiedBy { get; set; }
}
