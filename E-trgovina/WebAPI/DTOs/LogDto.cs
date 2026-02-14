using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    public class LogDto
    {
        public int Id { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        [Range(1, 5)] // 1-debug, 2-info, 3-warning, 4-error, 5- critical
        public int Level { get; set; }

        [Required]
        [StringLength(1024)]
        public string Message { get; set; } = null!;

        [StringLength(4096)]
        public string? Details { get; set; }
    }
}