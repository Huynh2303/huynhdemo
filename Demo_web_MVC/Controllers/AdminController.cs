using Demo_web_MVC.Models;
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
        public async Task<IActionResult> ProductManagerDetail(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            var model = await _adminDashboardService.GetProductManagerDetailAsync(id);

            return PartialView("ProductManagerDetail", model);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteProductByAdmin(int productId)
        {
            await _adminDashboardService.DeleteProductByAdminAsync(productId);

            var model = await _adminDashboardService.GetProductManagementAsync(1, 10);

            return PartialView("ProductManagement", model);
        }
        // 
        [HttpPost]
        public async Task<IActionResult> ConfirmOrder(int orderId)
        {
            await _adminDashboardService.ConfirmOrderAsync(orderId);

            var model = await _adminDashboardService
                .GetOrderManagementAsync(1, 10);

            return PartialView("OrderManagement", model);
        }
        [HttpPost]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            await _adminDashboardService.CancelOrderAsync(orderId);

            var model = await _adminDashboardService
                .GetOrderManagementAsync(1, 10);

            return PartialView("OrderManagement", model);
        }
        [HttpGet]
        public async Task<IActionResult> UpdateOrderStatusModal(int orderId)
        {
            var model = await _adminDashboardService
                .GetOrderDetailManagementAsync(orderId);

            if (model == null)
            {
                return NotFound();
            }

            return PartialView("UpdateOrderStatus", model);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus newStatus)
        {
            await _adminDashboardService
                .UpdateOrderStatusAsync(orderId, newStatus);

            var model = await _adminDashboardService
                .GetOrderManagementAsync(1, 10);

            return PartialView("OrderManagement", model);
        }
        public async Task<IActionResult> OrderRiskAnalysis(int orderId)
        {
            var model = await _adminDashboardService
                .GetOrderRiskAnalysisAsync(orderId);

            if (model == null)
            {
                return NotFound();
            }

            return PartialView("OrderRiskAnalysis", model);
        }
    }
}
