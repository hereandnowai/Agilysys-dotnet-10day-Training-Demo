using Microsoft.EntityFrameworkCore;
using Products.Data;
using Products.Models;

namespace Products.Repositories;

/// <summary>
/// Repository implementation for Product entity operations.
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductRepository"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public ProductRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products
            .AsNoTracking()
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductId == id);
    }

    /// <inheritdoc />
    public async Task<Product?> GetBySkuAsync(string sku)
    {
        return await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.SKU == sku);
    }

    /// <inheritdoc />
    public async Task<bool> SkuExistsAsync(string sku, int? excludeProductId = null)
    {
        var query = _context.Products.AsNoTracking().Where(p => p.SKU == sku);
        
        if (excludeProductId.HasValue)
        {
            query = query.Where(p => p.ProductId != excludeProductId.Value);
        }

        return await query.AnyAsync();
    }

    /// <inheritdoc />
    public async Task<Product> AddAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    /// <inheritdoc />
    public async Task<Product> UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
        return product;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Product product)
    {
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }
}
