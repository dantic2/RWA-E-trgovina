namespace WebApp.ViewModels
{
    public class CategoryAdminIndexViewModel
    {
        //filter
        public string? SearchTerm { get; set; }
        //pagination
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 10;
        public int TotalCategories { get; set; }
        //results
        public List<CategoryViewModel> Categories { get; set; } = new();
    }
}
