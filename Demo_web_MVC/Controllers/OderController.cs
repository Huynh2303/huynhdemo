using Demo_web_MVC.Service.Oder;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Demo_web_MVC.Controllers
{
    public class OderController : Controller
    {
        private readonly IOderService _service;
        private readonly ILogger<OderController> _logger;
        public OderController (IOderService service, ILogger<OderController> logger)
        {
            _service = service;
            _logger = logger;
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
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Details (int orderId)
        {
            var userId = GetUserIdFromClaims();
            if (userId == null)
            {
                return Unauthorized("Không xác định được người dùng.");
            }
            var result = await _service.GetOrderDetailAsyncService(userId.Value, orderId);
            return View(result);
        }
        public IActionResult CreateOrder()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateOrder(string paymentMethod, List<int> selectedCartItemIds)
        {
            try
            {
                var userId = GetUserIdFromClaims();

                if (userId == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var orderId = await _service
                    .CreateOrderFromCartAsyncService(userId.Value, paymentMethod,selectedCartItemIds);

                TempData["SuccessMessage"] = "Đặt hàng thành công.";

                return RedirectToAction("Details", "Oder", new { orderId = orderId });
                
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", "Cart");
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", "Cart");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo đơn hàng");

                TempData["ErrorMessage"] = "Có lỗi xảy ra khi đặt hàng.";
                return RedirectToAction("Index", "Cart");
            }
        }
        public async Task<IActionResult> MyOrders()
        {
            var userId = GetUserIdFromClaims();

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = await _service.GetOrdersByUserAsyncService(userId.Value);

            return View(orders);
        }
        public IActionResult UpdateStatus()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderId, string status)
        {
            try
            {
                var result = await _service.UpdateOrderStatusAsyncService(orderId, status);

                if (!result)
                {
                    TempData["ErrorMessage"] = "Không cập nhật được trạng thái.";
                }
                else
                {
                    TempData["SuccessMessage"] = "Cập nhật thành công.";
                }

                return RedirectToAction("Details", "Oder", new { orderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi update status");

                TempData["ErrorMessage"] = "Có lỗi xảy ra.";
                return RedirectToAction("Details", "Oder", new { orderId });
            }
        }
        public IActionResult CancelOrder()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            try
            {
                var userId = GetUserIdFromClaims();

                if (userId == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var result = await _service.CancelOrderAsyncService(orderId, userId.Value);

                if (result)
                {
                    TempData["SuccessMessage"] = "Đã hủy đơn hàng.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể hủy đơn.";
                }

                return RedirectToAction("Details", "Oder", new { orderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hủy đơn");

                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Details", "Oder", new { orderId });
            }
        }
        [HttpGet]
        public async Task<IActionResult> Checkout(List<int> selectedCartItemIds)
        {
            var userId = GetUserIdFromClaims(); // Lấy userId từ Claims hoặc Session
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            if (selectedCartItemIds == null || !selectedCartItemIds.Any())
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một sản phẩm để thanh toán."; // Thông báo lỗi
                return RedirectToAction("Index", "Cart");  // Quay lại trang giỏ hàng
            }
            // Gọi Service để lấy dữ liệu Checkout (Sản phẩm trong giỏ hàng, Địa chỉ giao hàng, Tổng tiền)
            var model = await _service.CheckoutAsync(userId.Value, selectedCartItemIds);

            // Trả về view Checkout với model chứa giỏ hàng và thông tin cần thiết
            return View(model);
        }

    }
}
