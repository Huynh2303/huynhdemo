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
        private int? GetSellerIdFromClaims()
        {
            var sellerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(sellerId))
            {
                return null;
            }

            return int.Parse(sellerId);
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

        [AllowAnonymous]
        public async Task<IActionResult> Index(int? categoryId)
        {
            var cartCount = await GetCartCount();
            ViewBag.CartCount = cartCount;

            var products = categoryId.HasValue
                ? await _productService.GetProductsByCategoryAsync(categoryId)
                : await _productService.getAll();

            return View(products);
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
                .OrderBy(x => Guid.NewGuid()) 
                .Take(4)
                .ToList();
            var cartCount = await GetCartCount();
            ViewBag.CartCount = cartCount;
            return View(productDetails);
        }
    }
}
