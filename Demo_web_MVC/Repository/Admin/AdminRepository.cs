using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel;
using Demo_web_MVC.Models.ViewModel.Admin;
using Demo_web_MVC.Models.ViewModel.Dashboard;
using Demo_web_MVC.Models.ViewModel.Oder;
using Demo_web_MVC.Models.ViewModel.Product;
using Demo_web_MVC.Repository.Dashboard;
using Microsoft.EntityFrameworkCore;

namespace Demo_web_MVC.Repository.Admin
{
    public class AdminRepository: IAdminRepository
    {
        private readonly AppDatabase _context;
        private readonly ILogger<AdminRepository> _logger;
        public AdminRepository(AppDatabase context, ILogger<AdminRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<AdminViewModel> GetAdminDashboardAsync()
        {
            try
            {
                _logger.LogInformation("Bắt đầu lấy dữ liệu dashboard admin...");

                // Tổng doanh thu
                var totalRevenue = await _context.Orders
                    .Where(o => o.Status == OrderStatus.Completed)
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

                // Tổng đơn hàng
                var totalOrders = await _context.Orders
                    .CountAsync();

                // Tổng sản phẩm
                var totalProducts = await _context.Products
                    .CountAsync();

                // Tổng người dùng
                var totalUsers = await _context.Users
                    .CountAsync();

                // Đơn hàng gần đây
                var recentOrders = await _context.Orders
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(5)
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

                var result = new AdminViewModel
                {
                    TotalRevenue = totalRevenue,
                    TotalOrders = totalOrders,
                    TotalProducts = totalProducts,
                    TotalUsers = totalUsers,

                    oderViewModels = recentOrders
                };

                _logger.LogInformation("Lấy dữ liệu dashboard admin thành công.");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Lỗi khi lấy dashboard admin: {Message}",
                    ex.Message);

                throw;
            }
        }

    }
}
