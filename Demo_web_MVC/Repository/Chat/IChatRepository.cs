using Demo_web_MVC.Models;

namespace Demo_web_MVC.Repository.Chat
{
    public interface IChatRepository
    {
        Task<Conversation?> GetConversationByIdAsync(int conversationId);

        Task<List<Conversation>> GetConversationsByUserAsync(int userId);

        Task<List<Conversation>> GetSystemSupportConversationsAsync();
        Task<Conversation?> GetSystemSupportConversationByUserAsync(int userId);
        Task<Conversation?> GetOrderSellerConversationAsync(int orderId, int userId, int sellerId);
    }
}
