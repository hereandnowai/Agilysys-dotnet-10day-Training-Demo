using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace First_Sample_Project_Prompting.Models;

/// <summary>
/// View model for customer data transfer and validation.
/// </summary>
public class POSViewCustomer
{
    /// <summary>
    /// Gets or sets the customer's name.
    /// </summary>
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 200 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's email address.
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255, ErrorMessage = "Email must not exceed 255 characters")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's phone number (optional).
    /// </summary>
    [Phone(ErrorMessage = "Invalid phone number format")]
    [RegularExpression(@"^[\d\s\-\+\(\)]+$", ErrorMessage = "Phone number can only contain digits, spaces, dashes, plus signs, and parentheses")]
    [StringLength(20, ErrorMessage = "Phone number must not exceed 20 characters")]
    public string? Phone { get; set; }

    /// <summary>
    /// Gets or sets the customer's billing address.
    /// </summary>
    [Required(ErrorMessage = "Billing address is required")]
    [StringLength(500, MinimumLength = 1, ErrorMessage = "Billing address must be between 1 and 500 characters")]
    public string BillingAddress { get; set; } = string.Empty;

    /// <summary>
    /// Validates the input data and throws an exception if validation fails.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when validation fails with user-readable error messages.</exception>
    public void ValidateInput()
    {
        var validationErrors = new List<string>();

        // Validate Name
        if (string.IsNullOrWhiteSpace(Name))
        {
            validationErrors.Add("Name is required and cannot be empty");
        }
        else if (Name.Length > 200)
        {
            validationErrors.Add("Name must not exceed 200 characters");
        }

        // Validate Email
        if (string.IsNullOrWhiteSpace(Email))
        {
            validationErrors.Add("Email is required and cannot be empty");
        }
        else if (!IsValidEmail(Email))
        {
            validationErrors.Add("Email format is invalid. Please provide a valid email address");
        }
        else if (Email.Length > 255)
        {
            validationErrors.Add("Email must not exceed 255 characters");
        }

        // Validate Phone (if provided)
        if (!string.IsNullOrWhiteSpace(Phone))
        {
            if (!IsValidPhoneNumber(Phone))
            {
                validationErrors.Add("Phone number format is invalid");
            }
            else if (Phone.Length > 20)
            {
                validationErrors.Add("Phone number must not exceed 20 characters");
            }
        }

        // Validate billing_address
        if (string.IsNullOrWhiteSpace(BillingAddress))
        {
            validationErrors.Add("Billing address is required and cannot be empty");
        }
        else if (BillingAddress.Length > 500)
        {
            validationErrors.Add("Billing address must not exceed 500 characters");
        }

        // Throw exception if there are validation errors
        if (validationErrors.Any())
        {
            var errorMessage = string.Join("; ", validationErrors);
            throw new ArgumentException($"Validation failed: {errorMessage}");
        }
    }

    /// <summary>
    /// Validates email format using regex pattern.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns>True if email format is valid; otherwise, false.</returns>
    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Enhanced email validation pattern
            var emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(email, emailPattern, RegexOptions.IgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates phone number format.
    /// </summary>
    /// <param name="phoneNumber">The phone number to validate.</param>
    /// <returns>True if phone number format is valid; otherwise, false.</returns>
    private bool IsValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return true; // Phone number is optional

        // Phone number pattern: allows digits, spaces, dashes, plus signs, and parentheses
        var phonePattern = @"^[\d\s\-\+\(\)]+$";
        return Regex.IsMatch(phoneNumber, phonePattern);
    }

    /// <summary>
    /// Converts the view model to a POSCustomer entity.
    /// </summary>
    /// <returns>A new POSCustomer entity with data from this view model.</returns>
    public POSCustomer ToEntity()
    {
        return new POSCustomer
        {
            Name = Name?.Trim() ?? string.Empty,
            Email = Email?.Trim().ToLowerInvariant() ?? string.Empty,
            Phone = Phone?.Trim(),
            billing_address = BillingAddress?.Trim() ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates an existing POSCustomer entity with data from this view model.
    /// </summary>
    /// <param name="customer">The customer entity to update.</param>
    public void UpdateEntity(POSCustomer customer)
    {
        if (customer == null)
            throw new ArgumentNullException(nameof(customer));

        customer.Name = Name?.Trim() ?? string.Empty;
        customer.Email = Email?.Trim().ToLowerInvariant() ?? string.Empty;
        customer.Phone = Phone?.Trim();
        customer.billing_address = BillingAddress?.Trim() ?? string.Empty;
        customer.UpdatedAt = DateTime.UtcNow;
    }
}
