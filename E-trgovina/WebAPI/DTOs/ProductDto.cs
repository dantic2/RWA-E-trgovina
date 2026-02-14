using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(256)]
        public string Title { get; set; } = null!;

        [StringLength(2048)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 999999.99)]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stock is required")]
        [Range(0, 999999)]
        public int Stock { get; set; }

        [StringLength(512)]
        public string? ImageUrl { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public string? CategoryName { get; set; } 

        public List<int>? CountryIds { get; set; } = new();

        public List<string>? CountryNames { get; set; } = new();
    }
}