using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel;
using Demo_web_MVC.Models.ViewModel.Address;
using Demo_web_MVC.Models.ViewModel.Dashboard;
using Demo_web_MVC.Models.ViewModel.Oder;
using Demo_web_MVC.Models.ViewModel.Product;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using static Demo_web_MVC.Models.ViewModel.Dashboard.DashboardViewModel;

namespace Demo_web_MVC.Repository.Dashboard
{
    public class DashboardRepository:IDashboardRepository
    {
        private readonly AppDatabase _context;
        private readonly  ILogger<DashboardRepository> _logger;
        public DashboardRepository( AppDatabase context, ILogger<DashboardRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<DashboardViewModel> GetOrdersAndProductsAsync()
        {
            try
            {
                // Log khi bắt đầu lấy danh sách đơn hàng
                _logger.LogInformation("Bắt đầu lấy danh sách đơn hàng...");

                var orderIds = await _context.Orders
                    .OrderByDescending(o => o.CreatedAt)
                    .Select(o => new OderViewModel
                    {
                        Id = o.Id,
                        TotalAmount = o.TotalAmount,
                        Status = o.Status,
                        CreateAt = o.CreatedAt,
                        user = o.User.FullName,
                        FraudAnalysis = _context.FraudAnalyses
                            .Where(f => f.OrderId == o.Id)
                            .OrderByDescending(f => f.CreatedAt)
                            .Select(f => new FraudAnalysisViewModel
                            {
                                Id = f.Id,
                                OrderId = f.OrderId,
                                RiskScore = f.RiskScore,
                                RiskLevel = f.RiskLevel,
                                RiskReasons = f.RiskReasons,
                                ModelName = f.ModelName,
                                CreatedAt = f.CreatedAt
                            })
                            .FirstOrDefault(),
                        Items = o.OrderItems.Select(item => new OderItemViewModel
                        {
                            Name = item.Variant.Product.Name,
                            Price = item.Price,
                            Quantity = item.Quantity,
                            Img = item.Variant.ProductVariantImages
                                .OrderBy(img => img.SortOrder)
                                .Select(img => img.Url)
                                .FirstOrDefault()
                                ?? item.Variant.Product.ProductImages
                                    .Select(img => img.Url)
                                    .FirstOrDefault()
                                ?? "/uploads/images/no-image.jpg"
                        }).ToList()
                    })
                    .ToListAsync();

                _logger.LogInformation("Đã lấy xong danh sách đơn hàng, số lượng đơn hàng: {OrderCount}", orderIds.Count);

                // Log khi bắt đầu lấy danh sách sản phẩm
                _logger.LogInformation("Bắt đầu lấy danh sách sản phẩm...");

                var products = await _context.Products.Include(p=>p.ProductImages)
                    
                    .Select(p => new ProductViewModel
                    {
                        Id = p.Id,
                        CategoryId = p.CategoryId,
                        Name = p.Name,
                        imageUrl = p.ProductImages.Select(pi => pi.Url).ToList() ?? new List<string>(),
                        Variants = p.ProductVariants.Select(v => new ProductVariantsViewModel
                        {
                            Price = v.Price,
                            Stock = v.Stock,
                        }).ToList()
                    })
                    .ToListAsync();
                

                _logger.LogInformation("Đã lấy xong danh sách sản phẩm, số lượng sản phẩm: {ProductCount}", products.Count);

                // Tạo kết quả kết hợp và trả về
                var combinedResult = new DashboardViewModel
                {
                    Orders = orderIds,
                    Products = products
                };

                return combinedResult;
            }
            catch (Exception ex)
            {
                // Log lỗi nếu có ngoại lệ
                _logger.LogError(ex, "Lỗi khi lấy dữ liệu dashboard: {Message}", ex.Message);
                throw; // Ném lại lỗi để có thể xử lý ở nơi gọi phương thức
            }
        }
        public async Task<ProductsManagerViewModel> GetProductsManagerAsync()
        {
            // Lấy danh sách sản phẩm từ cơ sở dữ liệu
            var products = await _context.Products.Include(p => p.ProductImages)
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    CategoryId = p.CategoryId,
                    Name = p.Name,
                    imageUrl = p.ProductImages.Select(pi => pi.Url).ToList() ?? new List<string>(),
                    Variants = p.ProductVariants.Select(v => new ProductVariantsViewModel
                    {
                        Price = v.Price,
                        Stock = v.Stock,
                    }).ToList()
                })
                .ToListAsync();

            // Trả về ProductsManagerViewModel chứa danh sách sản phẩm
            return new ProductsManagerViewModel
            {
                Products = products
            };
        }
        public async Task<List<DetailsOrderDashboardViewmodel>> GetDetailsOrderDashboardViewmodelAsync(int orderId)
        {
            var order = await _context.Orders.Where(o => o.Id == orderId)
                .Include(o => o.User)
                    .ThenInclude(u => u.Addresses)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Variant)
                        .ThenInclude(v => v.Product).Select(o => new DetailsOrderDashboardViewmodel
                        {
                            OrderId = orderId,
                            Email = o.User.Email,
                            OrderStatus = o.Status.ToString(),
                            TotalAmount = o.TotalAmount,
                            CreatedAt = o.CreatedAt,
                            AddressView = o.User.Addresses.Select(a => new AddressViewModel
                            {
                                RecipientName = a.RecipientName,
                                PhoneNumber = a.PhoneNumber,
                                AddressLine = a.AddressLine,
                                City = a.City,
                                Country = a.Country

                            }).FirstOrDefault(),
                            OderItemViews = o.OrderItems.Select(oi => new OderItemViewModel
                            {
                                OrderId = oi.Id,
                                Name = oi.Variant.Product.Name,
                                Price = oi.Price,
                                Quantity = oi.Quantity,
                                Img = oi.Variant.ProductVariantImages
                            .OrderBy(img => img.SortOrder)
                            .Select(img => img.Url)
                            .FirstOrDefault()
                            ?? oi.Variant.Product.ProductImages
                                .Select(img => img.Url)
                                .FirstOrDefault()
                            ?? "/uploads/images/no-image.jpg"
                            }).ToList(),
                            FraudAnalysis = _context.FraudAnalyses
                                .Where(f => f.OrderId == o.Id)
                                .OrderByDescending(f => f.CreatedAt)
                                .Select(f => new FraudAnalysisViewModel
                                {
                                    Id = f.Id,
                                    OrderId = f.OrderId,
                                    RiskScore = f.RiskScore,
                                    RiskLevel = f.RiskLevel,
                                    RiskReasons = f.RiskReasons,
                                    ModelName = f.ModelName,
                                    CreatedAt = f.CreatedAt
                                })
                                .FirstOrDefault()
                        }).ToListAsync();


            if (!order.Any())
            {
                _logger.LogError("Không có OrderId này: {OrderId}", orderId);
                return new List<DetailsOrderDashboardViewmodel>();
            }

            foreach (var item in order)
            {
                if (item.FraudAnalysis != null &&
                    !string.IsNullOrEmpty(item.FraudAnalysis.RiskReasons))
                {
                    item.FraudAnalysis.Reasons =
                        JsonSerializer.Deserialize<List<string>>(item.FraudAnalysis.RiskReasons)
                        ?? new List<string>();
                }
            }



            return order;
        }
        public async Task<StatisticsViewModel> GetDashboardStatisticsAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.CreatedAt.Date == DateTime.Today) // chỉ lấy đơn hôm nay
                .ToListAsync();

            var statistics = new StatisticsViewModel();

            statistics.TotalOrders = orders.Count;
            statistics.TotalProducts = orders.Sum(o => o.OrderItems.Sum(i => i.Quantity));
            statistics.TotalRevenue = orders.Sum(o => o.TotalAmount);


            // --- Biểu đồ ngày: từng đơn hôm nay ---
            statistics.Orders = orders
                .Select(o => new OderViewModel
                {
                    CreateAt = o.CreatedAt,
                    TotalAmount = o.OrderItems.Sum(i => i.Price * i.Quantity)
                })
                .OrderBy(o => o.CreateAt)
                .ToList();

            // --- Biểu đồ 7 ngày gần nhất: tổng theo ngày ---
            var last7Orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.CreatedAt >= DateTime.Today.AddDays(-6))
                .ToListAsync();

            statistics.RevenueLast7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-6 + i))
                .ToDictionary(
                    d => d,
                    d => last7Orders.Where(o => o.CreatedAt.Date == d)
                                     .Sum(o => o.OrderItems.Sum(i => i.Price * i.Quantity))
                );

            // --- Biểu đồ 30 ngày gần nhất: tổng theo ngày ---
            var last30Orders = await _context.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.CreatedAt >= DateTime.Today.AddDays(-29))
                .ToListAsync();

            statistics.RevenueLast30Days = Enumerable.Range(0, 30)
                .Select(i => DateTime.Today.AddDays(-29 + i))
                .ToDictionary(
                    d => d,
                    d => last30Orders.Where(o => o.CreatedAt.Date == d)
                                      .Sum(o => o.OrderItems.Sum(i => i.Price * i.Quantity))
                );

            // Thống kê trạng thái
            var allOrders = await _context.Orders
     .Include(o => o.OrderItems)
     .ToListAsync(); // lấy tất cả đơn, không chỉ hôm nay

            statistics.OrderStatusAll = allOrders
                .GroupBy(o => o.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            var last7Days = DateTime.Today.AddDays(-6);
            statistics.OrderStatusLast7Days = Enum.GetValues(typeof(OrderStatus))
                .Cast<OrderStatus>()
                .ToDictionary(
                    s => s,
                    s => allOrders.Count(o => o.Status == s && o.CreatedAt.Date >= last7Days)
                );

            var last30Days = DateTime.Today.AddDays(-29);
            statistics.OrderStatusLast30Days = Enum.GetValues(typeof(OrderStatus))
                .Cast<OrderStatus>()
                .ToDictionary(
                    s => s,
                    s => allOrders.Count(o => o.Status == s && o.CreatedAt.Date >= last30Days)
                );
            return statistics;
        }
    }
}
