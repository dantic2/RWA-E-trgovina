using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(256, MinimumLength = 8)]
        public string Password { get; set; } = null!;

        [Required]
        [StringLength(256)]
        public string FirstName { get; set; } = null!;

        [Required]
        [StringLength(256)]
        public string LastName { get; set; } = null!;

        [StringLength(512)]
        public string? Address { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Phone]
        public string? Phone { get; set; }
    }
}