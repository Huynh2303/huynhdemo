using Demo_web_MVC.Models.ViewModel.Admin;
using Demo_web_MVC.Models.ViewModel.Dashboard;

namespace Demo_web_MVC.Repository.Admin
{
    public interface IAdminRepository
    {
        Task<AdminViewModel> GetAdminDashboardAsync();
        Task<OderManagementViewModel> GetOrderManagementAsync(int page, int pageSize);
        Task<ProductManagementViewModel> GetProductManagementAsync(int page, int pageSize);
        Task<UserManagementViewModel> GetUserManagementAsync(int page, int pageSize);
        Task<CategoryManagementViewModel> GetCategoryManagementAsync(int page, int pageSize);
        Task<OrderDetailManagementViewModel?> GetOrderDetailManagementAsync(int orderId);
    }
}
