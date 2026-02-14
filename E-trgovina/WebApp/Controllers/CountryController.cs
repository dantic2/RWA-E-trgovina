using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;
using WebApp.ViewModels;

namespace WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CountryController : Controller
    {
        private readonly EcommerceDbContext _context;

        public CountryController(EcommerceDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchTerm, int page = 1)
        {
            int pageSize = 10;

            var query = _context.Countries.AsQueryable();

            // filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c => c.Name.Contains(searchTerm) || c.Code.Contains(searchTerm));
            }

            var totalCountries = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCountries / (double)pageSize);

            var countries = await query
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var countryViewModels = countries.Select(c => new CountryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code
            }).ToList();

            var viewModel = new CountryAdminIndexViewModel
            {
                SearchTerm = searchTerm,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize,
                TotalCountries = totalCountries,
                Countries = countryViewModels
            };

            return View(viewModel);
        }

        public IActionResult Create()
        {
            return View(new CountryViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CountryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {

                var existingByName = await _context.Countries
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == model.Name.ToLower());

                if (existingByName != null)
                {
                    ModelState.AddModelError("Name", $"Country with name '{model.Name}' already exists");
                    return View(model);
                }

                var existingByCode = await _context.Countries
                    .FirstOrDefaultAsync(c => c.Code.ToUpper() == model.Code.ToUpper());

                if (existingByCode != null)
                {
                    ModelState.AddModelError("Code", $"Country with code '{model.Code}' already exists");
                    return View(model);
                }

                var country = new Country
                {
                    Name = model.Name,
                    Code = model.Code
                };

                _context.Countries.Add(country);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating country: {ex.Message}");
                return View(model);
            }
        }

        // GET
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var country = await _context.Countries.FindAsync(id);

            if (country == null)
            {
                return NotFound();
            }

            var viewModel = new CountryViewModel
            {
                Id = country.Id,
                Name = country.Name,
                Code = country.Code
            };

            return View(viewModel);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CountryViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var existingByName = await _context.Countries
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == model.Name.ToLower() && c.Id != id);

                if (existingByName != null)
                {
                    ModelState.AddModelError("Name", $"Country with name '{model.Name}' already exists");
                    return View(model);
                }

                var existingByCode = await _context.Countries
                    .FirstOrDefaultAsync(c => c.Code.ToUpper() == model.Code.ToUpper() && c.Id != id);

                if (existingByCode != null)
                {
                    ModelState.AddModelError("Code", $"Country with code '{model.Code}' already exists");
                    return View(model);
                }

                var country = await _context.Countries.FindAsync(id);

                if (country == null)
                {
                    return NotFound();
                }

                country.Name = model.Name;
                country.Code = model.Code;

                _context.Update(country);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CountryExists(model.Id))
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating country: {ex.Message}");
                return View(model);
            }
        }

        //  GET
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var country = await _context.Countries
                .FirstOrDefaultAsync(c => c.Id == id);

            if (country == null)
            {
                return NotFound();
            }

            var viewModel = new CountryViewModel
            {
                Id = country.Id,
                Name = country.Name,
                Code = country.Code
            };

            return View(viewModel);
        }

        //  POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var country = await _context.Countries
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (country == null)
                {
                    return NotFound();
                }

                // check if country is used by any products
                if (country.Products.Any())
                {
                    ModelState.AddModelError("", $"Cannot delete country '{country.Name}' because it is used by {country.Products.Count} product(s)");

                    var viewModel = new CountryViewModel
                    {
                        Id = country.Id,
                        Name = country.Name,
                        Code = country.Code
                    };

                    return View(viewModel);
                }

                _context.Countries.Remove(country);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error deleting country: {ex.Message}");
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        //   
        private bool CountryExists(int id)
        {
            return _context.Countries.Any(c => c.Id == id);
        }
    }
}