using Demo_web_MVC.Models.ViewModel.Admin;
using Demo_web_MVC.Repository.Admin;
using System;

namespace Demo_web_MVC.Service.Admin
{
    public class AdminService: IAdminService
    {
        private readonly IAdminRepository _adminRepository;
        private readonly ILogger<AdminService> _logger;

        public AdminService(
            IAdminRepository adminRepository,
            ILogger<AdminService> logger)
        {
            _adminRepository = adminRepository;
            _logger = logger;
        }
        public async Task<AdminViewModel> GetAdminDashboardAsync()
        {
            try
            {
                _logger.LogInformation("Service bắt đầu lấy dashboard admin...");

                var dashboard = await _adminRepository.GetAdminDashboardAsync();
                if (dashboard == null)
                {
                    return new AdminViewModel();
                }

                return dashboard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi service khi lấy dashboard admin.");
                throw;
            }
        }
    }
}
