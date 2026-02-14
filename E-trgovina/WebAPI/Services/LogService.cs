using WebAPI.Models;

namespace WebAPI.Services
{
    public class LogService : ILogService
    {
        private readonly EcommerceDbContext _context;

        public LogService(EcommerceDbContext context)
        {
            _context = context;
        }

        public async Task LogDebug(string message, string? details = null)
        {
            await AddLog(1, message, details);  // debug
        }

        public async Task LogInfo(string message, string? details = null)
        {
            await AddLog(2, message, details);  // info
        }

        public async Task LogWarning(string message, string? details = null)
        {
            await AddLog(3, message, details);  //warning
        }

        public async Task LogError(string message, string? details = null)
        {
            await AddLog(4, message, details);  // error
        }

        public async Task LogCritical(string message, string? details = null)
        {
            await AddLog(5, message, details);  // critical
        }

        private async Task AddLog(int level, string message, string? details)
        {
            try
            {
                var log = new Log
                {
                    Timestamp = DateTime.UtcNow,
                    Level = level,
                    Message = message.Length > 1024 ? message.Substring(0, 1024) : message,
                    ErrorDetails = details
                };

                _context.Logs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LOGGING ERROR] {ex.Message}");
            }
        }
    }
}