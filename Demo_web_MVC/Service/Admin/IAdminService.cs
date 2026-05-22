using Demo_web_MVC.Models.ViewModel.Admin;

namespace Demo_web_MVC.Service.Admin
{
    public interface IAdminService
    {
        Task<AdminViewModel> GetAdminDashboardAsync();
    }
}
