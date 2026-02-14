using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    public class CountryDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(3)] // iso 3-letter code
        public string Code { get; set; } = null!;

        [Required]
        [StringLength(256)]
        public string Name { get; set; } = null!;
    }
}