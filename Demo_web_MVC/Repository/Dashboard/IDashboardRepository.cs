using Demo_web_MVC.Models.ViewModel.Dashboard;
using static Demo_web_MVC.Models.ViewModel.Dashboard.DashboardViewModel;

namespace Demo_web_MVC.Repository.Dashboard
{
    public interface IDashboardRepository
    {
        Task<DashboardViewModel> GetOrdersAndProductsAsync();
    }
}
