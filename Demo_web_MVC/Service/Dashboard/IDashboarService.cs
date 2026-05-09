using Demo_web_MVC.Models.ViewModel.Dashboard;
using static Demo_web_MVC.Models.ViewModel.Dashboard.DashboardViewModel;

namespace Demo_web_MVC.Service.Dashboard
{
    public interface IDashboarService
    {
        Task<DashboardViewModel> GetOrdersAndProductsAsync();
        Task<ProductsManagerViewModel> GetProductsManagerAsync();
        Task<List<DetailsOrderDashboardViewmodel>> GetDetailsOrderDashboardViewmodelAsync(int orderId);
    }
}
