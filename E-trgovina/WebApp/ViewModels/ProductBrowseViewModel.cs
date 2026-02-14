using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.ViewModels
{
    public class ProductBrowseViewModel
    {
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public int? CountryId { get; set; }

        //pagination
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 10;
        public int TotalProducts { get; set; }

        //results
        public List<ProductCatalogViewModel> Products { get; set; } = new();

        //filter dropdown
        public SelectList? Categories { get; set; }
        public SelectList? Countries { get; set; }
    }
}
