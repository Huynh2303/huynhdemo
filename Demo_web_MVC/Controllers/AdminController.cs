using Demo_web_MVC.Service.Admin;
using Microsoft.AspNetCore.Mvc;

namespace Demo_web_MVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminService _adminDashboardService;

        public AdminController(IAdminService adminDashboardService)
        {
            _adminDashboardService = adminDashboardService;
        }
        //private bool IsAjaxRequest()
        //{
        //    return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        //}
        public IActionResult Index()
        {
            
            return View();
        }
        public async Task<IActionResult> Dashboard()
        {
            

            var model = await _adminDashboardService.GetAdminDashboardAsync();

            return PartialView("Dashboard", model);
        }
        public async Task<IActionResult> OrderManagement(int page = 1)
        {
            int pageSize = 10;
            var model = await _adminDashboardService
        .GetOrderManagementAsync(page, pageSize);
            return PartialView("OrderManagement", model);
        }
        public async Task<IActionResult> ProductManagement(
            int page = 1)
        {
            int pageSize = 10;

            var model = await _adminDashboardService
                .GetProductManagementAsync(page, pageSize);

            return PartialView("ProductManagement", model);
        }
        public async Task<IActionResult> UserManagement(int page = 1)
        {
            int pageSize = 10;
            var model = await _adminDashboardService.GetUserManagementAsync(page, pageSize);

            return PartialView("UserManagement", model);
        }
        public async Task<IActionResult> CategoryManagement(int page = 1)
        {
            int pageSize = 5;

            var model = await _adminDashboardService.GetCategoryManagementAsync(page, pageSize);

            return PartialView("CategoryManagement", model);
        }
        public async Task<IActionResult> OrderDetailManagement(int orderId)
        {
            var model = await _adminDashboardService
                .GetOrderDetailManagementAsync(orderId);

            return PartialView("OrderDetailManagement", model);
        }
    }
}
