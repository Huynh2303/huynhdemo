using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Address;
using Demo_web_MVC.Models.ViewModel.Dashboard;
using Demo_web_MVC.Models.ViewModel.Oder;
using Demo_web_MVC.Models.ViewModel.Product;
using Microsoft.EntityFrameworkCore;
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
                            }).ToList()

                        }).ToListAsync();
                

            if (order == null)
            {
                _logger.LogError("không có Orderid này ");
                return null;
            }

            

            return order;
        }
    }
}
