using Demo_web_MVC.Service.Oder;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Demo_web_MVC.Controllers
{
    public class OderController : Controller
    {
        private readonly IOderService _service;
        public OderController (IOderService service)
        {
            _service = service;
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
        //public async Task<IActionResult> CreateOderAsync (string paymentMethod)
        //{
        //    var userId = GetUserIdFromClaims();
        //    if (userId == null)
        //    {
        //        return Unauthorized("Không xác định được người dùng.");
        //    }
        //    var result = await _service.CreateOrderFromCartAsyncService(userId.Value, paymentMethod);
        //    return result;
        //}
    }
}
