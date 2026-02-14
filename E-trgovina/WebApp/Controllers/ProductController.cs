using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Abstractions;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly EcommerceDbContext _context;
        
        public ProductController(EcommerceDbContext context)
        {
            _context = context;
        }

        // -------------------------- ADMIN CRUD operations --------------------------

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string? searchTerm, int? categoryId, int page = 1)
        {

            int pageSize = 10;

            // base query
            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            // filter Search

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.Title.Contains(searchTerm));
            }

            // filter Category
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // number of product after filtering
            var totalProducts = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            // pagination
            var products = await query
                .OrderBy(p => p.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // mapiranje to ViewModel
            var productViewModels = products.Select(p => new ProductViewModel
            {
                Id = p.Id,
                Title = p.Title,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                Description = p.Description,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name
            }).ToList();


            // wrap into AdminIndexViewModel
            var viewModel = new ProductAdminIndexViewModel
            {
                SearchTerm = searchTerm,
                CategoryId = categoryId,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize,
                TotalProducts = totalProducts,
                Products = productViewModels,
                Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name"),
            };

            return View(viewModel);
        }

        // GET: /Product/Create
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var model = new ProductViewModel
            {
                //Stock = 0, //TODO: remove it maybe?
                Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name"),
                Countries = await GetCountryCheckboxes()
            };

            return View(model);
        }

        // POST/ Product/Create
        [HttpPost]
        [Authorize (Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
                model.Countries = await GetCountryCheckboxes(model.SelectedCountryIds);
                return View(model);
            }

            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.Title.ToLower() == model.Title.ToLower() && p.DeletedAt == null);

            if (existingProduct != null)
            {
                ModelState.AddModelError("Title", $"Product with title '{model.Title}' already exists");
                model.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
                model.Countries = await GetCountryCheckboxes(model.SelectedCountryIds);
                return View(model);
            }

                try
            {
                var product = new Product
                {
                    Title = model.Title,
                    Price = model.Price,
                    ImageUrl = model.ImageUrl,
                    Description = model.Description,
                    Stock = model.Stock,
                    CategoryId = model.CategoryId,
                    CreatedAt = DateTime.UtcNow
                };

                //many to many countries
                if (model.SelectedCountryIds != null && model.SelectedCountryIds.Any())
                {
                    var countries = await _context.Countries
                        .Where(c => model.SelectedCountryIds.Contains(c.Id))
                        .ToListAsync();

                    product.Countries = countries;
                }
                ;

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating product: {ex.Message}");
                model.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
                model.Countries = await GetCountryCheckboxes(model.SelectedCountryIds);
                return View(model);
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Countries)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var model = new ProductViewModel
            {
                Id = product.Id,
                Title = product.Title,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                Description = product.Description,
                Stock = product.Stock,
                CategoryId = product.CategoryId,
                SelectedCountryIds = product.Countries.Select(c => c.Id).ToList(),
                Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name"),
                Countries = await GetCountryCheckboxes(product.Countries.Select(c => c.Id).ToList())
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                model.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
                model.Countries = await GetCountryCheckboxes(model.SelectedCountryIds);
                return View(model);
            }

            try
            {

                var existingProduct = await _context.Products
            .FirstOrDefaultAsync(p => p.Title.ToLower() == model.Title.ToLower() && p.Id != id && p.DeletedAt == null);

                if (existingProduct != null)
                {
                    ModelState.AddModelError("Title", $"Product with title '{model.Title}' already exists");
                    model.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
                    model.Countries = await GetCountryCheckboxes(model.SelectedCountryIds);
                    return View(model);
                }

                var product = await _context.Products
                    .Include(p => p.Countries)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    return NotFound();
                }

                product.Title = model.Title;
                product.Price = model.Price;
                product.ImageUrl = model.ImageUrl;
                product.Description = model.Description;
                product.Stock = model.Stock;
                product.CategoryId = model.CategoryId;

                // many to many countries update
                product.Countries.Clear();
                if (model.SelectedCountryIds != null && model.SelectedCountryIds.Any())
                {
                    var countries = await _context.Countries
                        .Where(c => model.SelectedCountryIds.Contains(c.Id))
                        .ToListAsync();

                    foreach (var country in countries)
                    {
                        product.Countries.Add(country);
                    }
                }

                _context.Update(product);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(model.Id))
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating product: {ex.Message}");
                model.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
                model.Countries = await GetCountryCheckboxes(model.SelectedCountryIds);
                return View(model);
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Countries)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var model = new ProductViewModel
            {
                Id = product.Id,
                Title = product.Title,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                Description = product.Description,
                CategoryId = product.CategoryId
            };

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);

                if (product == null)
                {
                    return NotFound();
                }

                var orderItemCount = await _context.OrderItems
                    .CountAsync(oi => oi.ProductId == id);

                if (orderItemCount > 0)
                {
                    ModelState.AddModelError("", $"Cannot delte product '{product.Title}' beacause it is used bt {orderItemCount} orders");

                    var model = new ProductViewModel
                    {
                        Id = product.Id,
                        Title = product.Title,
                        Price = product.Price,
                        ImageUrl = product.ImageUrl,
                        Description = product.Description,
                        CategoryId = product.CategoryId
                    };

                    return View("Delete", model);
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error deleting product: {ex.Message}");
                return RedirectToAction(nameof(Delete), new { id });
            }
        }


        // ------------------- Public Actions -------------------

        [AllowAnonymous]
        public async Task<IActionResult> Browse(string? searchTerm, int? categoryId, int? countryId, int page = 1)
        {
            int pageSize = 10;

            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Countries)
                .AsQueryable();

            // filter serach
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.Title.Contains(searchTerm));
            }

            // filter Category
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // filter Country
            if (countryId.HasValue) 
            {
                query = query.Where(p => p.Countries.Any(c => c.Id == countryId.Value));
            }

            var totalProducts = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            var products = await query
                .OrderBy(p => p.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var catalogItems = products.Select(p => new ProductCatalogViewModel
            {
                Id = p.Id,
                Title = p.Title,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                CategoryName = p.Category.Name,
                Stock = p.Stock
            }).ToList();

            var viewModel = new ProductBrowseViewModel
            {
                SearchTerm = searchTerm,
                CategoryId = categoryId,
                CountryId = countryId,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize,
                Products = catalogItems,
                Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name"),
                Countries = new SelectList(await _context.Countries.OrderBy(c => c.Name).ToListAsync(), "Id", "Name")
            };

            return View(viewModel);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Countries)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var model = new ProductCatalogViewModel
            {
                Id = product.Id,
                Title = product.Title,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                Description = product.Description,
                CategoryName = product.Category.Name,
                Stock = product.Stock,
                AvailableCountries = product.Countries.Select(c => c.Name).ToList()
            };

            return View(model);
        }

        // GET:for ajax
        public async Task<IActionResult> BrowsePartial(string? searchTerm, int? categoryId, int? countryId, int page = 1)
        {
            int pageSize = 10;

            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Countries)
                .AsQueryable();

            // filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.Title.Contains(searchTerm));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (countryId.HasValue && countryId.Value > 0) {
                query = query.Where(p => p.Countries.Any(c => c.Id == countryId.Value));
            }

            var totalProducts = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            var products = await query
                .OrderBy(p => p.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var productViewModels = products.Select(p => new ProductCatalogViewModel
            {
                Id = p.Id,
                Title = p.Title,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                CategoryName = p.Category.Name,
                Stock = p.Stock
            }).ToList();

            
            var viewModel = new ProductBrowseViewModel
            {
                SearchTerm = searchTerm,
                CategoryId = categoryId,
                CountryId = countryId,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize,
                TotalProducts = totalProducts,
                Products = productViewModels,
                Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name"),
                Countries = new SelectList(await _context.Countries.OrderBy(c => c.Name).ToListAsync(), "Id", "Name")
            };

            //return partial view with product grid
            return PartialView("_ProductGrid", viewModel);
        }


        // -------------- Helpers ----------------


        private async Task<List<CountryCheckBoxViewModel>> GetCountryCheckboxes(List<int>? selectedIds = null)
        {
            var countries = await _context.Countries.ToListAsync();

            return countries.Select(c => new CountryCheckBoxViewModel
            {
                Id = c.Id,
                Name = c.Name,
                IsSelected = selectedIds != null && selectedIds.Contains(c.Id)
            }).ToList();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
