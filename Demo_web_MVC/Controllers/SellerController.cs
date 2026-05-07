using Demo_web_MVC.Models.ViewModel.Category;
using Demo_web_MVC.Models.ViewModel.Product;
using Demo_web_MVC.Service;
using Demo_web_MVC.Service.Category;
using Demo_web_MVC.Service.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Demo_web_MVC.Controllers
{
    //[Authorize(Roles = "ADMIN, SEFF")]
    public class SellerController : Controller
    {
        private readonly IDashboarService _service;
        private readonly ILogger<SellerController> _logger;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        public SellerController(IDashboarService dashboarService, ILogger<SellerController> logger, IProductService productService, ICategoryService categoryService)
        {
            _service = dashboarService;
            _logger = logger;
            _productService = productService;
            _categoryService = categoryService;
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
        public async Task<IActionResult> Create()
        {
            // Lấy danh sách các danh mục từ cơ sở dữ liệu
            var categories = await _categoryService.GetAllCategories();
            var categoryViewModels = categories.Select(c => new CategoryViewModel
            {
                Id = c.Id,
                Name = c.Name
            }).ToList();

            // Tạo ProductViewModel và gán danh sách danh mục vào Categories
            var productViewModel = new ProductViewModel
            {
                Categories = categoryViewModels  // Truyền vào thuộc tính Categories
            };
            return View(productViewModel);
        }
        [HttpPost]
        public async Task<IActionResult> Create(ProductViewModel productVM, IFormFile[] imageUrl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra nếu người dùng tải lên hình ảnh
                    if (imageUrl != null && imageUrl.Length > 0)
                    {
                        var uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products");
                        if (!Directory.Exists(uploadsDirectory))
                        {
                            Directory.CreateDirectory(uploadsDirectory); // Tạo thư mục nếu chưa có
                        }

                        var fileNames = new List<string>();
                        foreach (var file in imageUrl)
                        {
                            if (file.Length > 0)
                            {
                                var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                                var filePath = Path.Combine(uploadsDirectory, fileName);

                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    await file.CopyToAsync(stream);
                                }

                                fileNames.Add(fileName); // Đường dẫn tương đối đúng
                            }
                        }
                        // Gán đường dẫn của các hình ảnh vào model
                        productVM.imageUrl = fileNames;

                    }

                    // Gọi phương thức Create từ service để tạo sản phẩm
                    var result = await _productService.creat(productVM);

                    if (result == null)
                    {
                        // Thêm thông báo lỗi chi tiết
                        ModelState.AddModelError("", "Không thể tạo sản phẩm, vui lòng thử lại.");
                    }
                    else
                    {
                        // Chuyển hướng nếu tạo sản phẩm thành công
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    // Ghi log hoặc xử lý lỗi nếu có ngoại lệ xảy ra
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                }
            }

            return View(productVM);
        }
    }
}
