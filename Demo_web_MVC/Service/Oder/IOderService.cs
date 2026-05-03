using Demo_web_MVC.Models.ViewModel.Oder;

namespace Demo_web_MVC.Service.Oder
{
    public interface IOderService
    {
        Task<OderViewModel?> GetOrderDetailAsyncService( int userId, int orderId);
        Task<int> CreateOrderFromCartAsyncService(int userId, string paymentMethod);
    }
}
