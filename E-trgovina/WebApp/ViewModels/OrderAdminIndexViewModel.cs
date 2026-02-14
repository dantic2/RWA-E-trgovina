using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.ViewModels
{
    public class OrderAdminIndexViewModel
    {
        // filter
        public string? SearchTerm { get; set; }  // search po customer name ili email
        public string? Status { get; set; }       // filter po Status

        // paginacija
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 10;
        public int TotalOrders { get; set; }

        // resultati
        public List<OrderViewModel> Orders { get; set; } = new();

        // dropdown
        public SelectList? StatusOptions { get; set; }
    }
}