using Demo_web_MVC.Models.ViewModel.Category;
using Demo_web_MVC.Models.ViewModel.Dashboard;
using Demo_web_MVC.Models.ViewModel.Product;
using Demo_web_MVC.Service;
using Demo_web_MVC.Service.Cart;
using Demo_web_MVC.Service.Category;
using Demo_web_MVC.Service.Dashboard;
using Demo_web_MVC.Service.Oder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Demo_web_MVC.Controllers
{
    [Authorize(Roles = "STAFF")]
    public class SellerController : Controller
    {
        private readonly IDashboarService _service;
        private readonly ILogger<SellerController> _logger;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IOderService _oderService;
        private readonly ICartService _cartService;
        public SellerController(IDashboarService dashboarService, ILogger<SellerController> logger, IProductService productService, ICategoryService categoryService,IOderService oderService, ICartService cartService)
        {
            _service = dashboarService;
            _logger = logger;
            _productService = productService;
            _categoryService = categoryService;
            _oderService = oderService;
            _cartService = cartService;
        }
        private int? GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return null;
            }
            return userId;
        }
        private async Task<int> GetCartCount()
        {
            var userId = GetUserIdFromClaims();
            if (userId == null)
            {
                return 0;
            }

            var cartItems = await _cartService.GetCartItems(userId.Value);
            return cartItems.Count;
        }
        public async Task<IActionResult> Dashboard()
        {
            
            var dashboardData = await _service.GetOrdersAndProductsAsync();

           
            var totalOrders = dashboardData.Orders.Count;
            var totalRevenue = dashboardData.Orders.Sum(o => o.TotalAmount);
            var today = DateTime.Today; 
            var ordersToday = dashboardData.Orders.Where(o => o.CreateAt == today).ToList();
            
            var totalOrdersToday = ordersToday.Count;
            var product = dashboardData.Products.Count;
            
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalOrdersToday = totalOrdersToday;
            ViewBag.Product = product;
            var cartCount = await GetCartCount();
            ViewBag.CartCount = cartCount;
            
            return View(dashboardData);
        }
        public async Task<IActionResult> ProductsManager()
        {
          
            var productsManagerViewModel = await _service.GetProductsManagerAsync();
            
            var totalProducts = productsManagerViewModel.Products.Count;
            var cartCount = await GetCartCount();
            ViewBag.CartCount = cartCount;


            
            var lowStockProductsCount = productsManagerViewModel.Products.Count(p => p.Variants!.Any(v=>v.Stock<5));

            ViewBag.TotalProducts = totalProducts;
            
            ViewBag.LowStockProducts = lowStockProductsCount;

            return View(productsManagerViewModel);
        }
        public async Task<IActionResult> CreateProduct()
        {
            var categories = await _categoryService.GetAllCategories();
            var categoryViewModels = categories.Select(c => new CategoryViewModel
            {
                Id = c.Id,
                Name = c.Name
            }).ToList();

            var productViewModel = new ProductViewModel
            {
                Categories = categoryViewModels  
            };
            var cartCount = await GetCartCount();
            ViewBag.CartCount = cartCount;

            return View(productViewModel);
        }
        [HttpPost]  
        public async Task<IActionResult> CreateProduct(ProductViewModel productVM, IFormFile[] imageUrl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Upload ảnh chính của sản phẩm
                    if (imageUrl != null && imageUrl.Length > 0)
                    {
                        var uploadsDirectory = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            "uploads",
                            "products"
                        );

                        if (!Directory.Exists(uploadsDirectory))
                        {
                            Directory.CreateDirectory(uploadsDirectory);
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

                                fileNames.Add(fileName);
                            }
                        }

                        productVM.imageUrl = fileNames;
                    }

                    // Upload ảnh riêng của từng variant
                    if (productVM.Variants != null && productVM.Variants.Any())
                    {
                        var variantUploadsDirectory = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            "uploads",
                            "variants"
                        );

                        if (!Directory.Exists(variantUploadsDirectory))
                        {
                            Directory.CreateDirectory(variantUploadsDirectory);
                        }

                        foreach (var variant in productVM.Variants)
                        {
                            variant.ImageUrlsVariants = new List<string>();

                            if (variant.ImageFiles != null && variant.ImageFiles.Any())
                            {
                                foreach (var file in variant.ImageFiles)
                                {
                                    if (file.Length > 0)
                                    {
                                        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                                        var filePath = Path.Combine(variantUploadsDirectory, fileName);

                                        using (var stream = new FileStream(filePath, FileMode.Create))
                                        {
                                            await file.CopyToAsync(stream);
                                        }

                                        variant.ImageUrlsVariants.Add($"/uploads/variants/{fileName}");
                                    }
                                }
                            }
                        }
                    }

                    var result = await _productService.creat(productVM);

                    if (result == null)
                    {
                        ModelState.AddModelError("", "Không thể tạo sản phẩm, vui lòng thử lại.");
                    }
                    else
                    {
                        return RedirectToAction("ProductsManager");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                }
            }

            var categories = await _categoryService.GetAllCategories();

            productVM.Categories = categories.Select(c => new CategoryViewModel
            {
                Id = c.Id,
                Name = c.Name
            }).ToList();

            return View(productVM);
        }
        public async Task<IActionResult> DetailsOrder(int orderId)
        {
            var orderDetails = await _service.GetDetailsOrderDashboardViewmodelAsync(orderId);

            if (orderDetails == null || !orderDetails.Any())
            {
                return NotFound();
            }

            return View(orderDetails);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId)
        {
            var result = await _oderService.CreateAsync(orderId);

            if (!result)
            {
                TempData["Error"] = "Không thể cập nhật trạng thái đơn hàng.";
                return RedirectToAction("DetailsOrder", "Seller", new { orderId = orderId });
            }

            TempData["Success"] = "Đơn hàng đã được chuyển sang trạng thái đang giao.";
            return RedirectToAction("DetailsOrder", "Seller", new { orderId = orderId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var result = await _oderService.DeleteOrderAsync(orderId);

            if (!result)
            {
                TempData["Error"] = "Không thể hủy đơn hàng.";
                return RedirectToAction("DetailsOrder","Seller", new { orderId = orderId });
            }

            TempData["Success"] = "Đơn hàng đã được hủy thành công.";
            return RedirectToAction("DetailsOrder", "Seller", new { orderId = orderId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var result = await _productService.delete(id);

            if (!result)
            {
                TempData["Error"] = "Không thể xóa sản phẩm.";
                return RedirectToAction("ProductsManager");
            }

            TempData["Success"] = "Đã xóa sản phẩm thành công.";
            return RedirectToAction("ProductsManager");
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.getbyid(id);

            if (product == null)
            {
                return NotFound();
            }

            product.Categories = await _categoryService.GetAllCategories();

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, ProductViewModel model, IFormFile[] imageUrl)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _categoryService.GetAllCategories();
                return View(model);
            }

            try
            {
                // Upload ảnh chính của sản phẩm nếu có chọn ảnh mới
                if (imageUrl != null && imageUrl.Length > 0)
                {
                    var uploadsDirectory = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "uploads",
                        "products"
                    );

                    if (!Directory.Exists(uploadsDirectory))
                    {
                        Directory.CreateDirectory(uploadsDirectory);
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

                            fileNames.Add($"/uploads/products/{fileName}");
                        }
                    }

                    model.imageUrl = fileNames;
                }

                // Upload ảnh riêng của từng variant nếu có chọn ảnh mới
                if (model.Variants != null && model.Variants.Any())
                {
                    var variantUploadsDirectory = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "uploads",
                        "variants"
                    );

                    if (!Directory.Exists(variantUploadsDirectory))
                    {
                        Directory.CreateDirectory(variantUploadsDirectory);
                    }

                    foreach (var variant in model.Variants)
                    {
                        variant.ImageUrlsVariants = new List<string>();

                        if (variant.ImageFiles != null && variant.ImageFiles.Any())
                        {
                            foreach (var file in variant.ImageFiles)
                            {
                                if (file.Length > 0)
                                {
                                    var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                                    var filePath = Path.Combine(variantUploadsDirectory, fileName);

                                    using (var stream = new FileStream(filePath, FileMode.Create))
                                    {
                                        await file.CopyToAsync(stream);
                                    }

                                    variant.ImageUrlsVariants.Add($"/uploads/variants/{fileName}");
                                }
                            }
                        }
                    }
                }

                await _productService.update(id, model);

                return RedirectToAction("ProductsManager");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");

                model.Categories = await _categoryService.GetAllCategories();

                return View(model);
            }
        }
        public async Task<IActionResult> Statistics()
        {
            var statistics = await _service.GetStatisticsAsync();
            var total = await _service.GetOrdersAndProductsAsync();
            ViewBag.TotalOrdersAllTime = total.Orders.Count;
            ViewBag.TotalRevenueAllTime = total.Orders.Sum(o => o.TotalAmount);
            ViewBag.TotalProductsAllTime = total.Products.Count;
            var cartCount = await GetCartCount();
            ViewBag.CartCount = cartCount;
            return View(statistics); 
        }
       
    }
}
