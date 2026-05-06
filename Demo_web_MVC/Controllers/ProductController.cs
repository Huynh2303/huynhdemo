using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Category;
using Demo_web_MVC.Models.ViewModel.Product;
using Demo_web_MVC.Service;
using Demo_web_MVC.Service.Cart;
using Demo_web_MVC.Service.Category;
using Demo_web_MVC.Service.Oder;
using Demo_web_MVC.Service.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Demo_web_MVC.Controllers
{

    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IOderService _OderService;
        private readonly ICartService _cartService;
        public ProductController(IProductService productService, ICategoryService categoryService,IOderService oderService,ICartService cartService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _OderService = oderService;
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
            var userId = GetUserIdFromClaims(); // Lấy userId từ claims
            if (userId == null)
            {
                return 0; // Trả về 0 nếu người dùng chưa đăng nhập
            }

            var cartItems = await _cartService.GetCartItems(userId.Value); // Lấy giỏ hàng của người dùng từ service
            return cartItems.Count; // Trả về số lượng sản phẩm trong giỏ
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var cartCount = await GetCartCount();
            ViewBag.CartCount = cartCount;
            return View(await _productService.getAll());
        }
       
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound("không có id ");
            }
            var productDetails = await _productService.details(id.Value);
            if (productDetails == null)
            {
                return NotFound("không tìm thấy sản phẩm");
            }
            var allProducts = await _productService.getAll();

            productDetails.RelatedProducts = allProducts
                .Where(p => p.Id != id.Value)
                .OrderBy(x => Guid.NewGuid()) // 🔥 random
                .Take(4)
                .ToList();
            return View(productDetails);
        }
        [Authorize(Roles = "ADMIN, SEFF")]
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
        [Authorize(Roles = "ADMIN, SEFF")]
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
        [HttpPost]
        [Authorize(Roles = "ADMIN, SEFF")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (id <= 0)
            {
                return NotFound("không có id ");
            }
            try
            {

                var result = await _productService.delete(id);
                if (!result)
                {
                    TempData["Error"] = "Không tìm thấy sản phẩm để xóa.";
                    return RedirectToAction(nameof(Index));
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Ghi log hoặc xử lý lỗi nếu có ngoại lệ xảy ra
                ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                Console.WriteLine($"Error deleting product: {ex.Message}");
                return RedirectToAction("Index");
            }
        }
        [HttpGet]
        [Authorize(Roles = "ADMIN, SEFF")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.getbyid(id);
            if (product == null)
                return NotFound();

            product.Categories = await _categoryService.GetAllCategories();
            return View(product);
        }
        
        [HttpPost]
        [Authorize(Roles = "ADMIN, SEFF")]
        public async Task<IActionResult> Edit(int id, ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _categoryService.GetAllCategories();
                return View(model);
            }
            await _productService.update(id, model);
            return RedirectToAction("Index");
        }
    }
}
