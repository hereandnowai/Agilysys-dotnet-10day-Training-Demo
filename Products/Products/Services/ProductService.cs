using Products.Models;
using Products.Repositories;

namespace Products.Services;

/// <summary>
/// Service implementation for managing product business logic.
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly ILogger<ProductService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductService"/> class.
    /// </summary>
    /// <param name="repository">The product repository.</param>
    /// <param name="logger">The logger instance.</param>
    public ProductService(IProductRepository repository, ILogger<ProductService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        try
        {
            _logger.LogInformation("Retrieving all products from service");
            var products = await _repository.GetAllAsync();
            _logger.LogInformation("Successfully retrieved {Count} products", products.Count());
            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all products in service");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Product?> GetProductByIdAsync(int id)
    {
        try
        {
            if (!IsValidProductId(id))
            {
                _logger.LogWarning("Invalid product ID provided: {Id}", id);
                return null;
            }

            _logger.LogInformation("Retrieving product with ID: {Id}", id);
            var product = await _repository.GetByIdAsync(id);
            
            if (product == null)
            {
                _logger.LogWarning("Product with ID {Id} not found", id);
            }
            else
            {
                _logger.LogInformation("Successfully retrieved product with ID: {Id}", id);
            }

            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving product with ID: {Id}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Product> CreateProductAsync(Product product)
    {
        try
        {
            _logger.LogInformation("Creating new product with SKU: {SKU}", product.SKU);

            // Check if SKU already exists
            var skuExists = await _repository.SkuExistsAsync(product.SKU);
            if (skuExists)
            {
                _logger.LogWarning("Duplicate SKU detected: {SKU}", product.SKU);
                throw new InvalidOperationException($"A product with SKU '{product.SKU}' already exists");
            }

            // Set audit fields
            product.CreatedOn = DateTime.UtcNow;
            product.ModifiedOn = null;
            product.ModifiedBy = null;

            var createdProduct = await _repository.AddAsync(product);
            _logger.LogInformation("Successfully created product with ID: {Id}", createdProduct.ProductId);
            
            return createdProduct;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating product");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<Product?> UpdateProductAsync(int id, Product product)
    {
        try
        {
            if (!IsValidProductId(id))
            {
                _logger.LogWarning("Invalid product ID provided: {Id}", id);
                return null;
            }

            if (id != product.ProductId)
            {
                _logger.LogWarning("Product ID mismatch. URL ID: {UrlId}, Body ID: {BodyId}", 
                    id, product.ProductId);
                return null;
            }

            _logger.LogInformation("Updating product with ID: {Id}", id);

            // Check if product exists
            var existingProduct = await _repository.GetByIdAsync(id);
            if (existingProduct == null)
            {
                _logger.LogWarning("Product with ID {Id} not found for update", id);
                throw new InvalidOperationException($"Product with ID {id} not found");
            }

            // Check if SKU is being changed and if it conflicts with another product
            if (existingProduct.SKU != product.SKU)
            {
                var skuExists = await _repository.SkuExistsAsync(product.SKU, id);
                if (skuExists)
                {
                    _logger.LogWarning("Duplicate SKU detected during update: {SKU}", product.SKU);
                    throw new InvalidOperationException($"A product with SKU '{product.SKU}' already exists");
                }
            }

            // Update properties
            existingProduct.ProductName = product.ProductName;
            existingProduct.Description = product.Description;
            existingProduct.UnitPrice = product.UnitPrice;
            existingProduct.SKU = product.SKU;
            existingProduct.Quantity = product.Quantity;
            existingProduct.ModifiedOn = DateTime.UtcNow;
            existingProduct.ModifiedBy = product.ModifiedBy;

            var updatedProduct = await _repository.UpdateAsync(existingProduct);
            _logger.LogInformation("Successfully updated product with ID: {Id}", id);
            
            return updatedProduct;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating product with ID: {Id}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteProductAsync(int id)
    {
        try
        {
            if (!IsValidProductId(id))
            {
                _logger.LogWarning("Invalid product ID provided for deletion: {Id}", id);
                return false;
            }

            _logger.LogInformation("Attempting to delete product with ID: {Id}", id);

            var product = await _repository.GetByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {Id} not found for deletion", id);
                return false;
            }

            await _repository.DeleteAsync(product);
            _logger.LogInformation("Successfully deleted product with ID: {Id}", id);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting product with ID: {Id}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public bool IsValidProductId(int id)
    {
        return id > 0;
    }
}
