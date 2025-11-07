using Microsoft.AspNetCore.Mvc;
using Products.Models;
using Products.Services;

namespace Products.Controllers;

/// <summary>
/// API controller for managing products.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductsController"/> class.
    /// </summary>
    /// <param name="productService">The product service.</param>
    /// <param name="logger">The logger instance.</param>
    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all products.
    /// </summary>
    /// <returns>A list of all products.</returns>
    /// <response code="200">Returns the list of products.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts()
    {
        try
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all products");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "An error occurred while retrieving products");
        }
    }

    /// <summary>
    /// Gets a specific product by ID.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <returns>The product with the specified ID.</returns>
    /// <response code="200">Returns the requested product.</response>
    /// <response code="404">If the product is not found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Product>> GetProductById(int id)
    {
        try
        {
            if (!_productService.IsValidProductId(id))
            {
                _logger.LogWarning("Invalid product ID provided: {Id}", id);
                return BadRequest("Product ID must be greater than zero");
            }

            _logger.LogInformation("Retrieving product with ID: {Id}", id);
            
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
            {
                _logger.LogWarning("Product with ID {Id} not found", id);
                return NotFound($"Product with ID {id} not found");
            }

            _logger.LogInformation("Successfully retrieved product with ID: {Id}", id);
            
            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving product with ID: {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "An error occurred while retrieving the product");
        }
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="product">The product to create.</param>
    /// <returns>The created product.</returns>
    /// <response code="201">Returns the newly created product.</response>
    /// <response code="400">If the product data is invalid.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product)
    {
        try
        {
            if (product == null)
            {
                _logger.LogWarning("Null product object received in create request");
                return BadRequest("Product object is null");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for create product request");
                return BadRequest(ModelState);
            }

            var createdProduct = await _productService.CreateProductAsync(product);

            _logger.LogInformation("Successfully created product with ID: {Id}", createdProduct.ProductId);

            return CreatedAtAction(
                nameof(GetProductById), 
                new { id = createdProduct.ProductId }, 
                createdProduct);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business logic error while creating product");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating product");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "An error occurred while creating the product");
        }
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">The product ID to update.</param>
    /// <param name="product">The updated product data.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the product was updated successfully.</response>
    /// <response code="400">If the product data is invalid.</response>
    /// <response code="404">If the product is not found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
    {
        try
        {
            if (product == null)
            {
                _logger.LogWarning("Null product object received in update request");
                return BadRequest("Product object is null");
            }

            if (!_productService.IsValidProductId(id))
            {
                _logger.LogWarning("Invalid product ID provided: {Id}", id);
                return BadRequest("Product ID must be greater than zero");
            }

            if (id != product.ProductId)
            {
                _logger.LogWarning("Product ID mismatch. URL ID: {UrlId}, Body ID: {BodyId}", 
                    id, product.ProductId);
                return BadRequest("Product ID mismatch");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for update product request");
                return BadRequest(ModelState);
            }

            var updatedProduct = await _productService.UpdateProductAsync(id, product);

            if (updatedProduct == null)
            {
                _logger.LogWarning("Product with ID {Id} not found for update", id);
                return NotFound($"Product with ID {id} not found");
            }

            _logger.LogInformation("Successfully updated product with ID: {Id}", id);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business logic error while updating product with ID: {Id}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating product with ID: {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "An error occurred while updating the product");
        }
    }

    /// <summary>
    /// Deletes a product.
    /// </summary>
    /// <param name="id">The product ID to delete.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the product was deleted successfully.</response>
    /// <response code="400">If the product ID is invalid.</response>
    /// <response code="404">If the product is not found.</response>
    /// <response code="500">If an internal server error occurs.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            if (!_productService.IsValidProductId(id))
            {
                _logger.LogWarning("Invalid product ID provided for deletion: {Id}", id);
                return BadRequest("Product ID must be greater than zero");
            }

            var deleted = await _productService.DeleteProductAsync(id);

            if (!deleted)
            {
                _logger.LogWarning("Product with ID {Id} not found for deletion", id);
                return NotFound($"Product with ID {id} not found");
            }

            _logger.LogInformation("Successfully deleted product with ID: {Id}", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting product with ID: {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "An error occurred while deleting the product");
        }
    }
}
