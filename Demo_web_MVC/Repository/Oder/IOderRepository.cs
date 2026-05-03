using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Oder;

namespace Demo_web_MVC.Repository.Oder
{
    public interface IOderRepository
    {
        Task<int> CreateOrderFromCartAsync(int userId, string paymentMethod);
        Task<Order> GetOrderByIdAsync( int orderId);
        Task<OderViewModel?> GetOrderDetailAsync(int userId, int orderId);
        Task<List<OderViewModel>> GetOrdersByUserAsync(int userId);
        //Task<List<OderViewModel>> GetAllOrdersAsync();

        Task<bool> UpdateOrderStatusAsync(int orderId, string status);
        //Task<bool> CancelOrderAsync(int orderId, int userId);

        //Task<bool> CreatePaymentAsync(int orderId, string paymentMethod, decimal amount, string status);
       

        //Task<decimal> CalculateOrderTotalAsync(int userId);
        //Task<List<OrderItemViewModel>> GetOrderItemsAsync(int orderId);
    }
}
