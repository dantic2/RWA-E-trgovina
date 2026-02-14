using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(256)]
        public string Name { get; set; } = null!;

        [StringLength(2048)]
        public string? Description { get; set; }
    }
}