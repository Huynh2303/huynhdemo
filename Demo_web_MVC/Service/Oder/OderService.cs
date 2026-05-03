using Demo_web_MVC.Models.ViewModel.Oder;
using Demo_web_MVC.Repository.Oder;

namespace Demo_web_MVC.Service.Oder
{
    public class OderService:IOderService
    {
        private readonly IOderRepository _oderRepository;
        public OderService (IOderRepository oderRepository)
        {
            _oderRepository = oderRepository;
        }
        public async Task<int> CreateOrderFromCartAsyncService(int userId, string paymentMethod)
        {
            return await _oderRepository.CreateOrderFromCartAsync(userId, paymentMethod);
        }
        public async Task<OderViewModel?> GetOrderDetailAsyncService( int userId, int orderId)
        {
            var result = await _oderRepository.GetOrderDetailAsync( userId,orderId);
            
            return result;
        }
    }
}
