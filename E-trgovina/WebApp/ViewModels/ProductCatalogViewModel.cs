using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.ViewModels
{
    //view model for public product catalog
    // one prodcut 
    public class ProductCatalogViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public string CategoryName { get; set; } = null!;
        public List<string> AvailableCountries { get; set; } = new();
        public int Stock { get; set; }

    }
}
