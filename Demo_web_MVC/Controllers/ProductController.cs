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
            var cartCount = await GetCartCount();
            ViewBag.CartCount = cartCount;
            return View(productDetails);
        }
        
        [HttpPost]
        [Authorize(Roles = "ADMIN, STAFF")]
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
        

        
    }
}
