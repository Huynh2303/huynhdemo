using Demo_web_MVC.Models.ViewModel.Oder;

namespace Demo_web_MVC.Service.Oder
{
    public interface IOderService
    {
        Task<OderViewModel?> GetOrderDetailAsyncService(int userId, int orderId);
        Task<int> CreateOrderFromCartAsyncService(int userId, string paymentMethod);

        //Task<Order> GetOrderByIdAsyncService(int orderId);

        Task<List<OderViewModel>> GetOrdersByUserAsyncService(int userId);
        Task<bool> UpdateOrderStatusAsyncService(int orderId, string status);
        Task<bool> CancelOrderAsyncService(int orderId, int userId);
        //Task<decimal> CalculateOrderTotalAsyncService(int userId);
    }
}
