using Products.Models;

namespace Products.Repositories;

/// <summary>
/// Repository interface for Product entity operations.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Gets all products from the database.
    /// </summary>
    /// <returns>A collection of all products.</returns>
    Task<IEnumerable<Product>> GetAllAsync();

    /// <summary>
    /// Gets a product by its ID.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <returns>The product if found, otherwise null.</returns>
    Task<Product?> GetByIdAsync(int id);

    /// <summary>
    /// Gets a product by its SKU.
    /// </summary>
    /// <param name="sku">The product SKU.</param>
    /// <returns>The product if found, otherwise null.</returns>
    Task<Product?> GetBySkuAsync(string sku);

    /// <summary>
    /// Checks if a product with the given SKU exists.
    /// </summary>
    /// <param name="sku">The product SKU.</param>
    /// <param name="excludeProductId">Optional product ID to exclude from the check.</param>
    /// <returns>True if the SKU exists, otherwise false.</returns>
    Task<bool> SkuExistsAsync(string sku, int? excludeProductId = null);

    /// <summary>
    /// Adds a new product to the database.
    /// </summary>
    /// <param name="product">The product to add.</param>
    /// <returns>The added product with generated ID.</returns>
    Task<Product> AddAsync(Product product);

    /// <summary>
    /// Updates an existing product in the database.
    /// </summary>
    /// <param name="product">The product to update.</param>
    /// <returns>The updated product.</returns>
    Task<Product> UpdateAsync(Product product);

    /// <summary>
    /// Deletes a product from the database.
    /// </summary>
    /// <param name="product">The product to delete.</param>
    Task DeleteAsync(Product product);
}
