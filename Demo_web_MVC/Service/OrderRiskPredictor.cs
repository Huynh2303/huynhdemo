using Demo_web_MVC.Models;
using Microsoft.ML;

namespace Demo_web_MVC.Service
{
    public class OrderRiskPredictor
    {
        private readonly MLContext _mlContext;
        private readonly ITransformer _model;

        public OrderRiskPredictor()
        {
            _mlContext = new MLContext();

            var modelPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "MLModels",
                "order_risk_model.zip"
            );

            _model = _mlContext.Model.Load(modelPath, out _);
        }

        public OrderRiskPrediction Predict(OrderRiskTrainingData input)
        {
            var predictionEngine = _mlContext.Model
                .CreatePredictionEngine<OrderRiskTrainingData, OrderRiskPrediction>(_model);

            return predictionEngine.Predict(input);
        }

        public string GetRiskLevel(OrderRiskTrainingData input, OrderRiskPrediction prediction)
        {
            return GetRiskDecision(input, prediction).RiskLevel;
        }

        public string GetSuggestion(string riskLevel)
        {
            if (riskLevel == "DataWarning")
            {
                return "Đơn thiếu dữ liệu sản phẩm hoặc tổng tiền, cần kiểm tra lại dữ liệu đơn hàng.";
            }

            if (riskLevel == "High")
            {
                return "Nên gọi xác nhận khách hàng trước khi chuyển đơn sang Shipping.";
            }

            if (riskLevel == "Medium")
            {
                return "Nên kiểm tra lại thông tin khách hàng trước khi nhận đơn.";
            }

            return "Có thể nhận đơn.";
        }

        public OrderRiskDecision GetRiskDecision(OrderRiskTrainingData input, OrderRiskPrediction prediction)
        {
            var reasons = new List<string>();

            if (input.CurrentOrderValue <= 0 || input.ItemCount <= 0 || input.TotalQuantity <= 0)
            {
                reasons.Add("Đơn hàng thiếu dữ liệu sản phẩm, tổng tiền hoặc số lượng.");

                return new OrderRiskDecision
                {
                    RiskLevel = "DataWarning",
                    Suggestion = GetSuggestion("DataWarning"),
                    Reasons = reasons
                };
            }

            // HIGH: dấu hiệu mạnh
            if (input.OrdersLast24h >= 3)
            {
                reasons.Add($"Khách đã đặt {input.OrdersLast24h} đơn trong 24 giờ gần đây.");
            }

            if (input.OrdersLast24h >= 2 && input.CancelRateLast24h >= 1)
            {
                reasons.Add("Khách đặt nhiều đơn trong 24 giờ và toàn bộ đơn gần đây đều bị hủy.");
            }

            if (input.TotalOrders >= 3 && input.CancelRate >= 0.5f)
            {
                reasons.Add($"Tỷ lệ hủy đơn tổng thể cao: {input.CancelRate:P0}.");
            }

            if (input.OrdersLast7d >= 5 && input.CancelRateLast7d >= 0.5f)
            {
                reasons.Add($"Trong 7 ngày gần đây khách đặt {input.OrdersLast7d} đơn và tỷ lệ hủy là {input.CancelRateLast7d:P0}.");
            }

            if (input.PhoneUsedCount >= 3)
            {
                reasons.Add($"Số điện thoại nhận hàng đang được dùng bởi {input.PhoneUsedCount} tài khoản.");
            }

            if (input.AddressUsedCount >= 3)
            {
                reasons.Add($"Địa chỉ nhận hàng đang được dùng bởi {input.AddressUsedCount} tài khoản.");
            }

            if (input.AccountAgeDays <= 7 && input.IsCod == 1 && input.CurrentOrderValue >= 3000000)
            {
                reasons.Add("Tài khoản mới, thanh toán COD và đơn hàng có giá trị cao.");
            }

            if (input.AccountAgeDays <= 7 && input.IsCod == 1 && input.TotalQuantity >= 15)
            {
                reasons.Add($"Tài khoản mới, thanh toán COD và đặt số lượng lớn: {input.TotalQuantity} sản phẩm.");
            }

            if (reasons.Any())
            {
                return new OrderRiskDecision
                {
                    RiskLevel = "High",
                    Suggestion = GetSuggestion("High"),
                    Reasons = reasons
                };
            }

            // MEDIUM: vùng cần kiểm tra thêm
            if (input.PhoneUsedCount == 2)
            {
                reasons.Add("Số điện thoại nhận hàng đã xuất hiện ở 2 tài khoản khác nhau.");
            }

            if (input.AddressUsedCount == 2)
            {
                reasons.Add("Địa chỉ nhận hàng đã xuất hiện ở 2 tài khoản khác nhau.");
            }

            if (input.StatusChangeCount >= 4)
            {
                reasons.Add($"Đơn hàng có số lần thay đổi trạng thái cao: {input.StatusChangeCount} lần.");
            }

            if (input.TotalOrders >= 3 && input.CancelRate >= 0.3f)
            {
                reasons.Add($"Tỷ lệ hủy đơn ở mức cần chú ý: {input.CancelRate:P0}.");
            }

            if (input.OrdersLast7d >= 4 && input.CancelRateLast7d >= 0.4f)
            {
                reasons.Add($"Trong 7 ngày gần đây tỷ lệ hủy đơn ở mức cần chú ý: {input.CancelRateLast7d:P0}.");
            }

            if (input.AvgOrderValue > 0 &&
                input.CurrentOrderValue >= input.AvgOrderValue * 3 &&
                input.CurrentOrderValue >= 1000000)
            {
                reasons.Add("Giá trị đơn hiện tại cao hơn nhiều so với giá trị đơn trung bình của khách.");
            }

            if (input.AccountAgeDays <= 7 && input.IsCod == 0 && input.TotalQuantity >= 25)
            {
                reasons.Add($"Tài khoản mới đặt số lượng sản phẩm lớn: {input.TotalQuantity} sản phẩm.");
            }

            if (prediction.IsRisk)
            {
                reasons.Add("Mô hình AI phát hiện mẫu hành vi có dấu hiệu rủi ro.");
            }

            if (reasons.Any())
            {
                return new OrderRiskDecision
                {
                    RiskLevel = "Medium",
                    Suggestion = GetSuggestion("Medium"),
                    Reasons = reasons
                };
            }

            return new OrderRiskDecision
            {
                RiskLevel = "Low",
                Suggestion = GetSuggestion("Low"),
                Reasons = new List<string>
                {
                    "Không phát hiện dấu hiệu rủi ro đáng chú ý."
                }
            };
        }
    }
}