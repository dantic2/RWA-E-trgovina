namespace WebAPI.Services
{
    public interface ILogService
    {
        Task LogDebug(string message, string? details = null);
        Task LogInfo(string message, string? details = null);
        Task LogWarning(string message, string? details = null);
        Task LogError(string message, string? details = null);
        Task LogCritical(string message, string? details = null);
    }
}
