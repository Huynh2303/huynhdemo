using Demo_web_MVC.Models;

namespace Demo_web_MVC.Service.Chat
{
    public interface IChatService
    {
        Task<Conversation> GetOrCreateSystemSupportConversationAsync(int userId);
        Task<Conversation> GetOrCreateOrderSellerConversationAsync(int orderId, int userId, int sellerId);
        Task<List<Conversation>> GetConversationsAsync(int userId, string role);
        Task<Conversation?> GetConversationDetailAsync(int conversationId, int userId, string role);
        Task<ChatMessage> SendMessageAsync(int conversationId, int senderId, string role, string content);
    }
}
