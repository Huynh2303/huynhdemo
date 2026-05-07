using Demo_web_MVC.Service.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace Demo_web_MVC.Controllers
{
    public class SellerController : Controller
    {
        private readonly IDashboarService _service;
        private readonly ILogger<SellerController> _logger;
        public SellerController(IDashboarService dashboarService, ILogger<SellerController> logger)
        {
            _service = dashboarService;
            _logger = logger;
        }
        // Dashboard action để trả về trang dashboard của người bán
        //public async Task<IActionResult> Dashboard()
        //{
        //    // Đảm bảo gọi await để lấy kết quả từ phương thức bất đồng bộ
        //    var dashboardData = await _service.GetOrdersAndProductsAsync();


        //    // Trả về View với dữ liệu thực tế (dashboardData) thay vì Task
        //    return View(dashboardData);
        //}
        public async Task<IActionResult> Dashboard()
        {
            // Đảm bảo gọi await để lấy kết quả từ phương thức bất đồng bộ
            var dashboardData = await _service.GetOrdersAndProductsAsync();

            // Tính tổng số đơn hàng và doanh thu
            var totalOrders = dashboardData.Orders.Count;
            var totalRevenue = dashboardData.Orders.Sum(o => o.TotalAmount);
            var today = DateTime.Today; // Lấy ngày hôm nay (không có giờ phút giây)
            var ordersToday = dashboardData.Orders.Where(o => o.CreateAt == today).ToList();
            // Tính tổng số đơn hàng hôm nay
            var totalOrdersToday = ordersToday.Count;
            var product = dashboardData.Products.Count;
            // Truyền tổng vào ViewBag hoặc ViewModel
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalOrdersToday = totalOrdersToday;
            ViewBag.Product = product;

            // Trả về View với dữ liệu thực tế (dashboardData)
            return View(dashboardData);
        }
    }
}
