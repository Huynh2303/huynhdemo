using Demo_web_MVC.Service.Chat;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;


namespace Demo_web_MVC.Controllers
{
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;
        private readonly IHubContext<ChatHub> _chatHub;
        public ChatController(IChatService chatService, IHubContext<ChatHub> chatHub)
        {
            _chatService = chatService;
            _chatHub = chatHub;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var role = GetRole();

            var conversations = await _chatService.GetConversationsAsync(userId, role);

            return View(conversations);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var userId = GetUserId();
            var role = GetRole();

            var conversation = await _chatService.GetConversationDetailAsync(id, userId, role);

            if (conversation == null)
                return NotFound();

            return View(conversation);
        }

        public async Task<IActionResult> Support()
        {
            var userId = GetUserId();

            var conversation = await _chatService
                .GetOrCreateSystemSupportConversationAsync(userId);

            return RedirectToAction("Detail", new { id = conversation.Id });
        }

        public async Task<IActionResult> OrderSeller(int orderId, int sellerId)
        {
            var userId = GetUserId();

            var conversation = await _chatService
                .GetOrCreateOrderSellerConversationAsync(orderId, userId, sellerId);

            return RedirectToAction("Detail", new { id = conversation.Id });
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(int conversationId, string content)
        {
            var userId = GetUserId();
            var role = GetRole();

            var message = await _chatService.SendMessageAsync(
                conversationId,
                userId,
                role,
                content);

            await _chatHub.Clients
                .Group($"conversation-{conversationId}")
                .SendAsync("ReceiveMessage", new
                {
                    conversationId = conversationId,
                    senderId = message.SenderId,
                    senderName = User.Identity?.Name ?? "Người dùng",
                    content = message.Content,
                    createdAt = message.CreatedAt.ToString("dd/MM/yyyy HH:mm")
                });

            return Ok();
        }

        private int GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                throw new Exception("Không tìm thấy UserId.");

            return int.Parse(userId);
        }

        private string GetRole()
        {
            return User.FindFirstValue(ClaimTypes.Role) ?? "";
        }

    }
}
