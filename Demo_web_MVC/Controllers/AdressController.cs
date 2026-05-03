using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Address;
using Demo_web_MVC.Service.Address;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Demo_web_MVC.Controllers
{
    public class AdressController : Controller
    {
        public readonly IAddressService _addressService;
        public readonly ILogger<AdressController> _logger;
        public AdressController(IAddressService addressService, ILogger<AdressController> logger)
        {
            _addressService = addressService;
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
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (userId == null)
                {
                    return Unauthorized("Không xác định được người dùng.");
                }

                var result = (await _addressService.GetAllByUserId(userId.Value)).ToList();

                _logger.LogInformation(
                    "Lấy danh sách địa chỉ thành công cho userId: {UserId}, Số lượng địa chỉ: {Count}",
                    userId.Value,
                    result.Count
                );

                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách địa chỉ.");
                return StatusCode(500, "Đã xảy ra lỗi khi tải danh sách địa chỉ.");
            }
        }
        public IActionResult Create()
        {
            return View("Create");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddressViewModel model)
        {
            _logger.LogInformation("POST Create Address nhận model: {@Model}", model);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState invalid: {@ModelState}", ModelState);
                return View(model);
            }

            var userId = GetUserIdFromClaims();
            if (userId == null)
            {
                _logger.LogWarning("Người dùng chưa login hoặc claims không hợp lệ.");
                return Unauthorized("Không xác định được người dùng.");
            }

            try
            {
                var result = await _addressService.Create(userId.Value, model);
                if (!result)
                {
                    _logger.LogWarning(
                        "Tạo địa chỉ thất bại cho UserId {UserId}, Address: {Address}",
                        userId.Value,
                        model.AddressLine
                    );
                    ModelState.AddModelError("", "Không thể tạo địa chỉ mới. Vui lòng thử lại.");
                    return View(model);
                }

                _logger.LogInformation(
                    "Tạo địa chỉ mới thành công cho UserId {UserId}, Address: {Address}",
                    userId.Value,
                    model.AddressLine
                );
                TempData["SuccessMessage"] = "Tạo địa chỉ thành công!";
                return RedirectToAction("Index", "Adress");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo địa chỉ mới cho UserId {UserId}", userId.Value);
                return View("Error", new ErrorViewModel { RequestId = "Đã xảy ra lỗi khi tạo địa chỉ mới." });
            }
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == null)
            {
                return Unauthorized("Không xác định được người dùng.");
            }
            try
            {
                var result = await _addressService.Delete(userId.Value, id);
                if (!result)
                {
                    TempData["ErrorMessage"] = "Không thể xóa địa chỉ. Vui lòng thử lại.";
                }
                else
                {
                    TempData["SuccessMessage"] = "Xóa địa chỉ thành công!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa địa chỉ Id {AddressId} cho UserId {UserId}", id, userId.Value);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa địa chỉ.";
            }
            return RedirectToAction("Index", "Adress");
        }
        [HttpPost]
        public async Task<IActionResult> SetDefault(int id)
        {
            var userId = GetUserIdFromClaims();
            if (userId == null)
            {
                return Unauthorized("Không xác định được người dùng.");
            }
            try
            {
                var result = await _addressService.SetDefaultAddress(userId.Value, id);
                if (!result)
                {
                    TempData["ErrorMessage"] = "Không thể đặt địa chỉ mặc định. Vui lòng thử lại.";
                }
                else
                {
                    TempData["SuccessMessage"] = "Đặt địa chỉ mặc định thành công!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đặt địa chỉ Id {AddressId} làm mặc định cho UserId {UserId}", id, userId.Value);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi đặt địa chỉ mặc định.";
            }
            return RedirectToAction("Index", "Adress");
        }
        [HttpGet]
        public async Task<IActionResult>Edit (int id)
        {
            var userId = GetUserIdFromClaims();
            if ( userId == null)
            {
                _logger.LogWarning("Người dùng chưa login hoặc claims không hợp lệ.");
                return Unauthorized("Không xác định được người dùng.");
            }
            var address = await _addressService.GetById(userId.Value,id);
            if (address == null)
            {
                return NotFound();
            }

            return View("Create", address); // 🔥 dùng lại view Create
        }
        [HttpPost]
        public async Task<IActionResult> Edit (int id , AddressViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var userId = GetUserIdFromClaims();
            if (userId == null)
            {
                _logger.LogWarning("Người dùng chưa login hoặc claims không hợp lệ.");
                return Unauthorized("Không xác định được người dùng.");
            }
            try
            {
               
                var editer = await _addressService.Update(userId.Value, id, model);
                if (!editer)
                {
                    TempData["ErrorMessage"] = "Không thể cập nhật địa chỉ. Vui lòng thử lại.";
                }
                else
                {
                    TempData["SuccessMessage"] = "Cập nhật địa chỉ thành công!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật địa chỉ Id {AddressId} cho UserId {UserId}", id, userId.Value);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi cập nhật địa chỉ.";
            }

            return RedirectToAction("Index", "Adress");
        }
    }
}
