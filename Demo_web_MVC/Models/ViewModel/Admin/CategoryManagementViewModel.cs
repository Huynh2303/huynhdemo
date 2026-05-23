using Demo_web_MVC.Models.ViewModel.Category;

namespace Demo_web_MVC.Models.ViewModel.Admin
{
    public class CategoryManagementViewModel
    {
        public int TotalCategories { get; set; }

        public PaginatedList<CategoryViewModel>? Categories { get; set; }
    }
}
