using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Admin;
using Demo_web_MVC.Models.ViewModel.Oder;
using Demo_web_MVC.Models.ViewModel.Product;
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
        public async Task<OderManagementViewModel> GetOrderManagementAsync(int page, int pageSize)
        {
            var model = await _adminRepository
            .GetOrderManagementAsync(page, pageSize);

            if (model == null)
            {
                return new OderManagementViewModel
                {
                    Orders = new PaginatedList<OderViewModel>(
                        new List<OderViewModel>(),
                        0,
                        page,
                        pageSize
                    )
                };
            }

            return model;
        }
        public async Task<ProductManagementViewModel> GetProductManagementAsync(int page,int pageSize)
        {
            if (page < 1)
            {
                page = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 10;
            }

            var model = await _adminRepository
                .GetProductManagementAsync(page, pageSize);

            if (model == null)
            {
                return new ProductManagementViewModel
                {
                    Products = new PaginatedList<ProductViewModel>(
                        new List<ProductViewModel>(),
                        0,
                        page,
                        pageSize
                    )
                };
            }
            model.Products ??= new PaginatedList<ProductViewModel>(
                new List<ProductViewModel>(),
                0,
                page,
                pageSize
            );

            return model;
        }
        public async Task<UserManagementViewModel> GetUserManagementAsync(int page, int pageSize)
        {
            if (page <= 0)
            {
                page = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 10;
            }

            var model = await _adminRepository.GetUserManagementAsync(page, pageSize);

            if (model == null)
            {
                return new UserManagementViewModel
                {
                    Users = new PaginatedList<UserItemViewModel>(
                        new List<UserItemViewModel>(),
                        0,
                        page,
                        pageSize
                    )
                };
            }

            return model;
        }
    }
}
