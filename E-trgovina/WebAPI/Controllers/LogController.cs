using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.DTOs;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly EcommerceDbContext _context;

        public LogController(EcommerceDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LogDto>>> GetLogs([FromQuery] int last = 20)
        {
            try
            {
                var logs = await _context.Logs
                    .OrderByDescending(l => l.Timestamp)
                    .Take(last)
                    .Select(l => new LogDto
                    {
                        Id = l.Id,
                        Timestamp = l.Timestamp,
                        Level = l.Level,
                        Message = l.Message,
                        Details = l.ErrorDetails
                    })
                    .ToListAsync();

                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}