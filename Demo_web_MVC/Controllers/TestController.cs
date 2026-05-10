using Demo_web_MVC.Repository.OrderRisk;
using Microsoft.AspNetCore.Mvc;

namespace Demo_web_MVC.Controllers
{
    public class TestController : Controller
    {
        private readonly IOrderRiskRepository _orderRepository;

        public TestController(IOrderRiskRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
       
        public async Task<IActionResult> TestRiskInput(int orderId)
        {
            var data = await _orderRepository.BuildRiskInputAsync(orderId);

            if (data == null)
            {
                return NotFound("Không tìm thấy đơn hàng");
            }

            return Json(data);
        }
    }
}
