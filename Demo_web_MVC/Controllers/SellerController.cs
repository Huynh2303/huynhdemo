using Demo_web_MVC.Service;
using Demo_web_MVC.Service.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace Demo_web_MVC.Controllers
{
    public class SellerController : Controller
    {
        private readonly IDashboarService _service;
        private readonly ILogger<SellerController> _logger;
        private readonly IProductService _productService;
        public SellerController(IDashboarService dashboarService, ILogger<SellerController> logger, IProductService productService)
        {
            _service = dashboarService;
            _logger = logger;
            _productService = productService;
        }

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
        public async Task<IActionResult> ProductsManager()
        {
            // Gọi phương thức trong service để lấy dữ liệu
            var productsManagerViewModel = await _service.GetProductsManagerAsync();
            // Tính tổng số sản phẩm
            var totalProducts = productsManagerViewModel.Products.Count;

            

            // Tính tổng sản phẩm sắp hết hàng (tồn kho dưới 5)
            var lowStockProductsCount = productsManagerViewModel.Products.Count(p => p.Variants!.Any(v=>v.Stock<5));

            // Truyền các giá trị vào ViewBag
            ViewBag.TotalProducts = totalProducts;
            
            ViewBag.LowStockProducts = lowStockProductsCount;

            // Trả về view với dữ liệu từ service
            return View(productsManagerViewModel);
        }

    }
}
