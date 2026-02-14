using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    public class ChangePasswordDto
    {
        [Required]
        public string OldPassword { get; set; } = null!;

        [Required]
        [StringLength(256, MinimumLength = 8)]
        public string NewPassword { get; set; } = null!;
    }
}