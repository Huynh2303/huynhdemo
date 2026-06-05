using Demo_web_MVC.Service.Notifications;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Demo_web_MVC.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly INotificationsService _notificationService;

        public NotificationsController(INotificationsService notificationService)
        {
            _notificationService = notificationService;
        }
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();

            var notifications = await _notificationService
                .GetUserNotificationsAsync(userId);

            return View(notifications);
        }

        public async Task<IActionResult> Dropdown()
        {
            var userId = GetUserId();

            var notifications = await _notificationService
                .GetUserNotificationsAsync(userId);

            return PartialView("_NotificationDropdown", notifications.Take(5).ToList());
        }

        public async Task<IActionResult> UnreadCount()
        {
            var userId = GetUserId();

            var count = await _notificationService
                .GetUnreadCountAsync(userId);

            return Json(count);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetUserId();

            await _notificationService.MarkAsReadAsync(id, userId);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetUserId();

            await _notificationService.MarkAllAsReadAsync(userId);

            return Ok();
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }
    }
}
