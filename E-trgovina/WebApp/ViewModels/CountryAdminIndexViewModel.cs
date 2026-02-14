namespace WebApp.ViewModels
{
    public class CountryAdminIndexViewModel
    {
        //filter
        public string? SearchTerm { get; set; }

        //pagination
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 10;
        public int TotalCountries { get; set; }

        //results
        public List<CountryViewModel> Countries { get; set; } = new();
    }
}
