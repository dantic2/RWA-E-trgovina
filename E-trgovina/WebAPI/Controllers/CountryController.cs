using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.DTOs;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        private readonly EcommerceDbContext _context;

        public CountryController(EcommerceDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CountryDto>>> GetCountries()
        {
            try
            {
                var countries = await _context.Countries
                    .Select(c => new CountryDto
                    {
                        Id = c.Id,
                        Code = c.Code,
                        Name = c.Name
                    })
                    .ToListAsync();

                return Ok(countries);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<CountryDto>> GetCountry(int id)
        {
            try
            {
                var country = await _context.Countries.FindAsync(id);

                if (country == null)
                    return NotFound($"Country with id={id} not found");

                var countryDto = new CountryDto
                {
                    Id = country.Id,
                    Code = country.Code,
                    Name = country.Name
                };

                return Ok(countryDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<CountryDto>> PostCountry([FromBody] CountryDto countryDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // check if country with the same code already exists
                var exists = await _context.Countries.AnyAsync(c => c.Code == countryDto.Code);
                if (exists)
                    return BadRequest($"Country with code '{countryDto.Code}' already exists");

                var country = new Country
                {
                    Code = countryDto.Code,
                    Name = countryDto.Name
                };

                _context.Countries.Add(country);
                await _context.SaveChangesAsync();

                countryDto.Id = country.Id;

                return CreatedAtAction(nameof(GetCountry), new { id = country.Id }, countryDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<CountryDto>> PutCountry(int id, [FromBody] CountryDto countryDto)
        {
            try
            {
                if (id != countryDto.Id)
                    return BadRequest("ID mismatch");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var country = await _context.Countries.FindAsync(id);

                if (country == null)
                    return NotFound($"Country with id={id} not found");

                // check
                var exists = await _context.Countries.AnyAsync(c => c.Code == countryDto.Code && c.Id != id);
                if (exists)
                    return BadRequest($"Country with code '{countryDto.Code}' already exists");

                country.Code = countryDto.Code;
                country.Name = countryDto.Name;

                await _context.SaveChangesAsync();

                return Ok(countryDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            try
            {
                var country = await _context.Countries
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (country == null)
                    return NotFound($"Country with id={id} not found");

                // check if country has associated products
                if (country.Products.Any())
                    return BadRequest("Cannot delete country with existing products");

                _context.Countries.Remove(country);
                await _context.SaveChangesAsync();

                return Ok($"Country id={id} deleted");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}