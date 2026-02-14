using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels
{
    public class CountryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Country name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Display(Name = "Country Name")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Country code is required")]
        [StringLength(3, MinimumLength = 2, ErrorMessage = "Code must be 2-3 characters")]
        [RegularExpression(@"^[A-Z]{2,10}$", ErrorMessage = "Code must be uppercase letters only (e.g., HR, USA)")]
        [Display(Name = "Country Code")]
        public string Code { get; set; } = null!;
    }
}