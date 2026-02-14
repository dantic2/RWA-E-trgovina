using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels
{
    //view model for admin crud product
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        [Display(Name = "Product Title")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 99999.99, ErrorMessage = "Price must be between 0.01 and 99999.99")]
        [Display(Name = "Price (€)")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Display(Name = "Image URL")]
        [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
        [DataType(DataType.ImageUrl)] 
        public string? ImageUrl { get; set; }

        [Display(Name = "Description")]
        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Stock quantity is required")]
        [Range(0, 999999, ErrorMessage = "Stock must be between 0 and 999999")]
        [Display(Name = "Stock Quantity")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Display(Name = "Category Name")]
        public string? CategoryName { get; set; }

        [Display(Name = "Available in Countries")]
        public List<int>? SelectedCountryIds { get; set; } = new();

        //dropdown list
        public SelectList? Categories { get; set; }
        public List<CountryCheckBoxViewModel>? Countries { get; set; } 

        }

    //helper za country checkboxes
    public class CountryCheckBoxViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public bool IsSelected { get; set; }
    }
}
