using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Models;
using WebAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly EcommerceDbContext _context;
        private readonly ILogService _logService;

        public ProductController(EcommerceDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {

            await _logService.LogInfo("Fetching all products");  // LOG

            try
            {
                var products = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Countries)
                    .Where(p => p.DeletedAt == null)
                    .Select(p => new ProductDto
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Description = p.Description,
                        Price = p.Price,
                        Stock = p.Stock,
                        ImageUrl = p.ImageUrl,
                        CategoryId = p.CategoryId,
                        CategoryName = p.Category.Name,
                        CountryIds = p.Countries.Select(c => c.Id).ToList(),
                        CountryNames = p.Countries.Select(c => c.Name).ToList()
                    })
                    .ToListAsync();

                await _logService.LogInfo($"Returned {products.Count} products");

                return Ok(products);
            }
            catch (Exception ex)
            {
                await _logService.LogError($"Error fetching products: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Countries)
                    .Where(p => p.DeletedAt == null)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    await _logService.LogWarning($"Product id={id} not found");  //  LOG
                    return NotFound($"Product with id={id} not found");
                }

                await _logService.LogInfo($"Product id={id} retrieved:   {product.Title}"); // LOG

                var productDto = new ProductDto
                {
                    Id = product.Id,
                    Title = product.Title,
                    Description = product.Description,
                    Price = product.Price,
                    Stock = product.Stock,
                    ImageUrl = product.ImageUrl,
                    CategoryId = product.CategoryId,
                    CategoryName = product.Category.Name,
                    CountryIds = product.Countries.Select(c => c.Id).ToList(),
                    CountryNames = product.Countries.Select(c => c.Name).ToList()
                };

                return Ok(productDto);
            }
            catch (Exception ex)
            {
                await _logService.LogError($"Error retrieving product id={id}: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> Search(
            [FromQuery] string? query,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                await _logService.LogInfo($"Search started:  query='{query ?? "(all)"}', page={page}, pageSize={pageSize}");

                var productsQuery = _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Countries)
                    .Where(p => p.DeletedAt == null)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(query))
                {
                    productsQuery = productsQuery.Where(p =>
                        p.Title.Contains(query) ||
                        p.Description.Contains(query));
                }

                var totalCount = await productsQuery.CountAsync();
                
                var products = await productsQuery
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new ProductDto
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Description = p.Description,
                        Price = p.Price,
                        Stock = p.Stock,
                        ImageUrl = p.ImageUrl,
                        CategoryId = p.CategoryId,
                        CategoryName = p.Category.Name,
                        CountryIds = p.Countries.Select(c => c.Id).ToList(),
                        CountryNames = p.Countries.Select(c => c.Name).ToList()
                    })
                    .ToListAsync();

                await _logService.LogInfo($"Search completed: query='{query ?? "(all)"}', page={page}, returned {products.Count} of {totalCount} total products");

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<ProductDto>> PostProduct([FromBody] ProductDto productDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await _logService.LogWarning("Product creation failed: Invalid model state");
                    return BadRequest(ModelState);
                }

                var product = new Product
                {
                    Title = productDto.Title,
                    Description = productDto.Description,
                    Price = productDto.Price,
                    Stock = productDto.Stock,
                    ImageUrl = productDto.ImageUrl,
                    CategoryId = productDto.CategoryId,
                    CreatedAt = DateTime.UtcNow
                };

                // m-n relationship
                if (productDto.CountryIds != null && productDto.CountryIds.Any())
                {
                    var countries = await _context.Countries
                        .Where(c => productDto.CountryIds.Contains(c.Id))
                        .ToListAsync();

                    product.Countries = countries;
                }

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                productDto.Id = product.Id;

                await _logService.LogInfo($"Product id={product.Id} created: '{product.Title}', price={product.Price}, category={product.CategoryId}"); //LOG

                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, productDto);
            }
            catch (Exception ex)
            {
                await _logService.LogError($"Error creating product '{productDto.Title}': {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<ProductDto>> PutProduct(int id, [FromBody] ProductDto productDto)
        {
            try
            {
                if (id != productDto.Id)
                {
                    await _logService.LogWarning($"Product update failed: ID mismatch (url={id}, body={productDto.Id})");
                    return BadRequest("ID mismatch");
                }


                if (!ModelState.IsValid)
                {
                    await _logService.LogWarning($"Product id={id} update failed: Invalid model state");
                    return BadRequest(ModelState);
                }
            
                var product = await _context.Products
                    .Include(p => p.Countries)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null || product.DeletedAt != null)
                {
                    await _logService.LogWarning($"Product id={id} update failed:  Not found");
                    return NotFound($"Product with id={id} not found");
                }

                var oldTitle = product.Title;

                // update 
                product.Title = productDto.Title;
                product.Description = productDto.Description;
                product.Price = productDto.Price;
                product.Stock = productDto.Stock;
                product.ImageUrl = productDto.ImageUrl;
                product.CategoryId = productDto.CategoryId;

                // update m-m 
                product.Countries.Clear();
                if (productDto.CountryIds != null && productDto.CountryIds.Any())
                {
                    var countries = await _context.Countries
                        .Where(c => productDto.CountryIds.Contains(c.Id))
                        .ToListAsync();

                    foreach (var country in countries)
                    {
                        product.Countries.Add(country);
                    }
                }

                await _context.SaveChangesAsync();

                await _logService.LogInfo($"Product id={id} updated: '{oldTitle}' → '{product.Title}', price={product.Price}");

                return Ok(productDto);
            }
            catch (Exception ex)
            {
                await _logService.LogError($"Error updating product id={id}:  {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);

                if (product == null || product.DeletedAt != null)
                {
                    await _logService.LogWarning($"Product id={id} deletion failed: Not found");
                    return NotFound($"Product with id={id} not found");
                }

                var productTitle = product.Title;

                product.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                await _logService.LogInfo($"Product id={id} deleted (soft): '{productTitle}'");

                return Ok($"Product id={id} deleted");
            }
            catch (Exception ex)
            {
                await _logService.LogError($"Error deleting product id={id}:  {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }
    }
}