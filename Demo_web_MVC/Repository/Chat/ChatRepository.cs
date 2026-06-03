using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo_web_MVC.Repository.Chat
{
    public class ChatRepository : IChatRepository
    {
        private readonly AppDatabase _context;
        private readonly ILogger<ChatRepository> _logger;
        public ChatRepository (AppDatabase context, ILogger<ChatRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        //Lấy cuộc hội thoại theo ID không đồng bộ
        public async Task<Conversation?> GetConversationByIdAsync(int conversationId)
        {
            return await _context.Conversations
                .Include(x => x.Participants)
                    .ThenInclude(x => x.User)
                .Include(x => x.Messages)
                    .ThenInclude(x => x.Sender)
                .Include(x => x.Messages)
                    .ThenInclude(x => x.Attachments)
                .FirstOrDefaultAsync(x => x.Id == conversationId);
        }
        // lấy cuộc hội thoại theo user
        public async Task<List<Conversation>> GetConversationsByUserAsync(int userId)
        {
            return await _context.Conversations
                .Include(x => x.Participants)
                    .ThenInclude(x => x.User)
                .Where(x => x.Participants.Any(p => p.UserId == userId))
                .OrderByDescending(x => x.LastMessageAt ?? x.CreatedAt)
                .ToListAsync();
        }
        //lấy cuộc hội thoại hội trợ hệ thống của admin
        public async Task<List<Conversation>> GetSystemSupportConversationsAsync()
        {
            return await _context.Conversations
                .Include(x => x.Participants)
                    .ThenInclude(x => x.User)
                .Where(x => x.Type == "SystemSupport")
                .OrderByDescending(x => x.LastMessageAt ?? x.CreatedAt)
                .ToListAsync();
        }
        // lấy cuộc hội thoại hội trợ theo user
        public async Task<Conversation?> GetSystemSupportConversationByUserAsync(int userId)
        {
            return await _context.Conversations
                .Include(x => x.Participants)
                    .ThenInclude(x => x.User)
                .Include(x => x.Messages)
                    .ThenInclude(x => x.Sender)
                .Include(x => x.Messages)
                    .ThenInclude(x => x.Attachments)
                .FirstOrDefaultAsync(x =>
                    x.Type == "SystemSupport" &&
                    x.Participants.Any(p => p.UserId == userId));
        }
        //Nhận cuộc hội thoại giữa người bán và khách hàng
        public async Task<Conversation?> GetOrderSellerConversationAsync(int orderId, int userId, int sellerId)
        {
            return await _context.Conversations
                .Include(x => x.Participants)
                .FirstOrDefaultAsync(x =>
                    x.Type == "OrderSeller"
                    && x.OrderId == orderId
                    && x.Participants.Any(p => p.UserId == userId)
                    && x.Participants.Any(p => p.UserId == sellerId));
        }
    }
}
