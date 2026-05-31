using Demo_web_MVC.Models;
using Demo_web_MVC.Repository.OrderRisk;
using Demo_web_MVC.Service;
using Demo_web_MVC.Service.Birth;
using Microsoft.AspNetCore.Mvc;

namespace Demo_web_MVC.Controllers
{
    public class TestController : Controller
    {
        private readonly IOrderRiskRepository _orderRepository;

        private readonly OrderRiskModelTrainer _orderRiskModelTrainer;
        private readonly OrderRiskPredictor _orderRiskPredictor;
        private readonly IBirthService _birthService;
        private readonly ILogger<TestController > _logger;
        public TestController(ILogger<TestController> logger ,IBirthService birthService,IOrderRiskRepository orderRepository, OrderRiskModelTrainer orderRiskModelTrainer, OrderRiskPredictor orderRiskPredictor)
        {
            _orderRepository = orderRepository;
            _orderRiskModelTrainer = orderRiskModelTrainer;
            _orderRiskPredictor = orderRiskPredictor;
            _birthService = birthService;
            _logger = logger;
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
        public IActionResult TrainOrderRiskModel()
        {
            var result = _orderRiskModelTrainer.Train();

            return Content(result);
        }

        public IActionResult TestPublicDatasetLikeCases()
        {
            var cases = new List<OrderRiskTrainingData>
    {
        
        new OrderRiskTrainingData
        {
            AccountAgeDays = 120,
            TotalOrders = 1,
            OrdersLast24h = 0,
            OrdersLast7d = 1,
            CancelledOrders = 0,
            CancelRate = 0,
            CurrentOrderValue = 180000,
            AvgOrderValue = 180000,
            IsCod = 0,
            CodOrderCount = 0,
            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            ItemCount = 2,
            TotalQuantity = 3,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0,
            CancelledOrdersLast7d = 0,
            CancelRateLast7d = 0
        },

        // Case 2: Khách mua nhiều lần, lịch sử ổn
        // Pattern: khách cũ, nhiều invoice, không hủy
        // Kỳ vọng: Low
        new OrderRiskTrainingData
        {
            AccountAgeDays = 240,
            TotalOrders = 12,
            OrdersLast24h = 0,
            OrdersLast7d = 2,
            CancelledOrders = 0,
            CancelRate = 0,
            CurrentOrderValue = 420000,
            AvgOrderValue = 390000,
            IsCod = 0,
            CodOrderCount = 0,
            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            ItemCount = 4,
            TotalQuantity = 6,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0,
            CancelledOrdersLast7d = 0,
            CancelRateLast7d = 0
        },

        // Case 3: Khách mua sỉ nhẹ
        // UCI Online Retail có nhiều khách wholesale, nên quantity có thể cao hơn
        // Kỳ vọng: Low hoặc Medium nhẹ
        new OrderRiskTrainingData
        {
            AccountAgeDays = 300,
            TotalOrders = 20,
            OrdersLast24h = 0,
            OrdersLast7d = 3,
            CancelledOrders = 1,
            CancelRate = 0.05f,
            CurrentOrderValue = 1200000,
            AvgOrderValue = 900000,
            IsCod = 0,
            CodOrderCount = 0,
            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            ItemCount = 8,
            TotalQuantity = 20,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0,
            CancelledOrdersLast7d = 0,
            CancelRateLast7d = 0
        },

        // Case 4: Khách cũ, có 1 đơn bị hủy trong lịch sử
        // Kỳ vọng: Low
        new OrderRiskTrainingData
        {
            AccountAgeDays = 180,
            TotalOrders = 10,
            OrdersLast24h = 0,
            OrdersLast7d = 2,
            CancelledOrders = 1,
            CancelRate = 0.1f,
            CurrentOrderValue = 350000,
            AvgOrderValue = 400000,
            IsCod = 0,
            CodOrderCount = 0,
            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            ItemCount = 3,
            TotalQuantity = 5,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0,
            CancelledOrdersLast7d = 0,
            CancelRateLast7d = 0
        },

        // Case 5: Khách có nhiều đơn gần đây nhưng chưa hủy nhiều
        // Pattern: khách mua thường xuyên trong tuần
        // Kỳ vọng: Low hoặc Medium nhẹ
        new OrderRiskTrainingData
        {
            AccountAgeDays = 90,
            TotalOrders = 18,
            OrdersLast24h = 1,
            OrdersLast7d = 6,
            CancelledOrders = 1,
            CancelRate = 0.056f,
            CurrentOrderValue = 560000,
            AvgOrderValue = 450000,
            IsCod = 0,
            CodOrderCount = 0,
            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            ItemCount = 5,
            TotalQuantity = 8,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0,
            CancelledOrdersLast7d = 1,
            CancelRateLast7d = 0.167f
        },

        // Case 6: Hóa đơn có giá trị cao hơn trung bình
        // Dataset thật có khách mua nhiều mặt hàng trong một invoice
        // Kỳ vọng: Medium
        new OrderRiskTrainingData
        {
            AccountAgeDays = 150,
            TotalOrders = 8,
            OrdersLast24h = 0,
            OrdersLast7d = 2,
            CancelledOrders = 1,
            CancelRate = 0.125f,
            CurrentOrderValue = 1800000,
            AvgOrderValue = 450000,
            IsCod = 0,
            CodOrderCount = 0,
            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            ItemCount = 12,
            TotalQuantity = 30,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0,
            CancelledOrdersLast7d = 0,
            CancelRateLast7d = 0
        },

        // Case 7: Khách có tỷ lệ hủy trung bình
        // Trong UCI, hóa đơn bắt đầu bằng C thường được xem là cancellation/return
        // Kỳ vọng: Medium
        new OrderRiskTrainingData
        {
            AccountAgeDays = 200,
            TotalOrders = 9,
            OrdersLast24h = 0,
            OrdersLast7d = 4,
            CancelledOrders = 3,
            CancelRate = 0.333f,
            CurrentOrderValue = 480000,
            AvgOrderValue = 430000,
            IsCod = 0,
            CodOrderCount = 0,
            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            ItemCount = 4,
            TotalQuantity = 7,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0,
            CancelledOrdersLast7d = 2,
            CancelRateLast7d = 0.5f
        },

        // Case 8: Khách có nhiều đơn hủy trong lịch sử
        // Kỳ vọng: High
        new OrderRiskTrainingData
        {
            AccountAgeDays = 160,
            TotalOrders = 10,
            OrdersLast24h = 0,
            OrdersLast7d = 5,
            CancelledOrders = 5,
            CancelRate = 0.5f,
            CurrentOrderValue = 520000,
            AvgOrderValue = 400000,
            IsCod = 0,
            CodOrderCount = 0,
            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            ItemCount = 5,
            TotalQuantity = 9,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0,
            CancelledOrdersLast7d = 3,
            CancelRateLast7d = 0.6f
        },

        // Case 9: Nhiều đơn trong 24h, giống khách đặt nhiều invoice gần nhau
        // Kỳ vọng: High theo rule của hệ thống bạn
        new OrderRiskTrainingData
        {
            AccountAgeDays = 60,
            TotalOrders = 6,
            OrdersLast24h = 3,
            OrdersLast7d = 6,
            CancelledOrders = 0,
            CancelRate = 0,
            CurrentOrderValue = 300000,
            AvgOrderValue = 280000,
            IsCod = 0,
            CodOrderCount = 0,
            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            ItemCount = 3,
            TotalQuantity = 4,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0,
            CancelledOrdersLast7d = 0,
            CancelRateLast7d = 0
        },

        // Case 10: Khách mới, mua đơn nhỏ, không COD
        // Kỳ vọng: Low
        new OrderRiskTrainingData
        {
            AccountAgeDays = 5,
            TotalOrders = 1,
            OrdersLast24h = 1,
            OrdersLast7d = 1,
            CancelledOrders = 0,
            CancelRate = 0,
            CurrentOrderValue = 220000,
            AvgOrderValue = 220000,
            IsCod = 0,
            CodOrderCount = 0,
            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            ItemCount = 2,
            TotalQuantity = 2,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0,
            CancelledOrdersLast7d = 0,
            CancelRateLast7d = 0
        },

        // Case 11: Khách mới, đơn lớn/số lượng cao, nhưng dataset public không có COD
        // Kỳ vọng: Medium, vì không COD nhưng giá trị và số lượng cao
        new OrderRiskTrainingData
        {
            AccountAgeDays = 6,
            TotalOrders = 1,
            OrdersLast24h = 1,
            OrdersLast7d = 1,
            CancelledOrders = 0,
            CancelRate = 0,
            CurrentOrderValue = 2500000,
            AvgOrderValue = 2500000,
            IsCod = 0,
            CodOrderCount = 0,
            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            ItemCount = 15,
            TotalQuantity = 40,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0,
            CancelledOrdersLast7d = 0,
            CancelRateLast7d = 0
        },

        // Case 12: Khách mua sỉ ổn định, nhiều đơn nhưng hủy thấp
        // Kỳ vọng: Low hoặc Medium nhẹ
        new OrderRiskTrainingData
        {
            AccountAgeDays = 365,
            TotalOrders = 45,
            OrdersLast24h = 1,
            OrdersLast7d = 7,
            CancelledOrders = 2,
            CancelRate = 0.044f,
            CurrentOrderValue = 1500000,
            AvgOrderValue = 1300000,
            IsCod = 0,
            CodOrderCount = 0,
            PhoneUsedCount = 0,
            AddressUsedCount = 0,
            ItemCount = 10,
            TotalQuantity = 35,
            StatusChangeCount = 0,
            CancelledOrdersLast24h = 0,
            CancelRateLast24h = 0,
            CancelledOrdersLast7d = 1,
            CancelRateLast7d = 0.143f
        }
    };

            var results = cases.Select((input, index) =>
            {
                var prediction = _orderRiskPredictor.Predict(input);

                var decision = _orderRiskPredictor.GetRiskDecision(input, prediction);

                return new
                {
                    Case = index + 1,
                    Source = "Mapped from public real retail dataset patterns",
                    MissingFieldsDefaultedToZero = new[]
                    {
                        "IsCod",
                        "CodOrderCount",
                        "PhoneUsedCount",
                        "AddressUsedCount",
                        "StatusChangeCount"
                    },
                    Expected = index switch
                    {
                        0 => "Low",
                        1 => "Low",
                        2 => "Low hoặc Medium nhẹ",
                        3 => "Low",
                        4 => "Low hoặc Medium nhẹ",
                        5 => "Medium",
                        6 => "Medium",
                        7 => "High",
                        8 => "High",
                        9 => "Low",
                        10 => "Medium",
                        11 => "Low hoặc Medium nhẹ",
                        _ => ""
                    },
                    Input = input,
                    Prediction = new
                    {
                        prediction.IsRisk,
                        prediction.Score,
                        RiskLevel = decision.RiskLevel,
                        Suggestion = decision.Suggestion,
                        Reasons = decision.Reasons
                    }
                };
            });

            return Json(results);
        }
        public async Task<IActionResult> TestBirthdayEmail()
        {
            _logger.LogWarning("Bắt đầu");
            
            await _birthService.SendBirthdayEmailsAsync();
            
            return Content("Đã chạy gửi mail sinh nhật");
        }
    }
}
