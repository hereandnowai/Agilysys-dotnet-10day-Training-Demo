using Products.Models;

namespace Products.Services;

/// <summary>
/// Service interface for managing product business logic.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Retrieves all products.
    /// </summary>
    /// <returns>A collection of all products.</returns>
    Task<IEnumerable<Product>> GetAllProductsAsync();

    /// <summary>
    /// Retrieves a product by its ID.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <returns>The product if found, otherwise null.</returns>
    Task<Product?> GetProductByIdAsync(int id);

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="product">The product to create.</param>
    /// <returns>The created product.</returns>
    /// <exception cref="InvalidOperationException">Thrown when SKU already exists.</exception>
    Task<Product> CreateProductAsync(Product product);

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">The product ID to update.</param>
    /// <param name="product">The updated product data.</param>
    /// <returns>The updated product if successful, otherwise null.</returns>
    /// <exception cref="InvalidOperationException">Thrown when SKU already exists or product not found.</exception>
    Task<Product?> UpdateProductAsync(int id, Product product);

    /// <summary>
    /// Deletes a product by its ID.
    /// </summary>
    /// <param name="id">The product ID to delete.</param>
    /// <returns>True if the product was deleted, false if not found.</returns>
    Task<bool> DeleteProductAsync(int id);

    /// <summary>
    /// Validates if a product ID is valid.
    /// </summary>
    /// <param name="id">The product ID to validate.</param>
    /// <returns>True if valid, otherwise false.</returns>
    bool IsValidProductId(int id);
}
