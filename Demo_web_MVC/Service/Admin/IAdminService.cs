using Demo_web_MVC.Models.ViewModel.Admin;

namespace Demo_web_MVC.Service.Admin
{
    public interface IAdminService
    {
        Task<AdminViewModel> GetAdminDashboardAsync();
        Task<OderManagementViewModel> GetOrderManagementAsync(int page, int pageSize);
        Task<ProductManagementViewModel> GetProductManagementAsync(int page,int pageSize);
    }
}
