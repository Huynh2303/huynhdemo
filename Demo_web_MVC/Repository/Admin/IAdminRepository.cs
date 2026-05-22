using Demo_web_MVC.Models.ViewModel.Admin;
using Demo_web_MVC.Models.ViewModel.Dashboard;

namespace Demo_web_MVC.Repository.Admin
{
    public interface IAdminRepository
    {
         Task<AdminViewModel> GetAdminDashboardAsync();
    }
}
