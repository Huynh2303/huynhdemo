using Demo_web_MVC.Models.ViewModel.Dashboard;
using static Demo_web_MVC.Models.ViewModel.Dashboard.DashboardViewModel;

namespace Demo_web_MVC.Repository.Dashboard
{
    public interface IDashboardRepository
    {
        Task<DashboardViewModel> GetOrdersAndProductsAsync(int sellerId);
        Task<ProductsManagerViewModel> GetProductsManagerAsync(int sellerId);
        Task<List<DetailsOrderDashboardViewmodel>> GetDetailsOrderDashboardViewmodelAsync(int orderId, int sellerId);
        Task<StatisticsViewModel> GetDashboardStatisticsAsync(int sellerId);
    }
}
