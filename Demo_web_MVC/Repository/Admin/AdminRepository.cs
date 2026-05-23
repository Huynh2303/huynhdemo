using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel;
using Demo_web_MVC.Models.ViewModel.Admin;
using Demo_web_MVC.Models.ViewModel.Category;
using Demo_web_MVC.Models.ViewModel.Dashboard;
using Demo_web_MVC.Models.ViewModel.Oder;
using Demo_web_MVC.Models.ViewModel.Product;
using Demo_web_MVC.Repository.Dashboard;
using Demo_web_MVC.Repository.Paging;
using Microsoft.EntityFrameworkCore;

namespace Demo_web_MVC.Repository.Admin
{
    public class AdminRepository: IAdminRepository
    {
        private readonly AppDatabase _context;
        private readonly ILogger<AdminRepository> _logger;
        private readonly IPagingReponsitory _pagingReponsitory;
        public AdminRepository(AppDatabase context, ILogger<AdminRepository> logger, IPagingReponsitory pagingReponsitory)
        {
            _context = context;
            _logger = logger;
            _pagingReponsitory = pagingReponsitory;
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
                    .Take(10)
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
        public async Task<OderManagementViewModel> GetOrderManagementAsync(int page, int pageSize)
        {
            var ordersQuery = _context.Orders
                .AsNoTracking();

            var totalOrders = await ordersQuery.CountAsync();

            var pendingOrders = await ordersQuery
                .CountAsync(o => o.Status == OrderStatus.Pending);

            var cancelledOrders = await ordersQuery
                .CountAsync(o => o.Status == OrderStatus.Cancelled);

            var revenue = await ordersQuery
                .Where(o => o.Status == OrderStatus.Completed)
                .SumAsync(o => o.TotalAmount);

           var orders = ordersQuery
    .OrderByDescending(o => o.CreatedAt)
    .Select(o => new OderViewModel
    {
        Id = o.Id,
        TotalAmount = o.TotalAmount,
        Status = o.Status,
        CreateAt = o.CreatedAt,
        PaymentMethod = o.PaymentMethod,

        user = o.User.FullName ?? o.User.Username,

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
    });
            var pagedOrders = await _pagingReponsitory.GetPagedDataAsync(
                orders,
                page,
                pageSize
            );
            return new OderManagementViewModel
            {
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                CancelledOrders = cancelledOrders,
                Revenue = revenue,
                Orders = pagedOrders
            };
        }
        public async Task<ProductManagementViewModel> GetProductManagementAsync(int page,int pageSize)
        {
            var productsQuery = _context.Products
                .AsNoTracking();

            // Dashboard stats
            var totalProducts = await productsQuery.CountAsync();

            var totalCategories = await _context.Categories
                .CountAsync();

            var lowStockProducts = await productsQuery
                .CountAsync(p => p.ProductVariants
                    .Sum(v => v.Stock) > 0
                    && p.ProductVariants.Sum(v => v.Stock) < 10);

            var outOfStockProducts = await productsQuery
                .CountAsync(p => p.ProductVariants
                    .Sum(v => v.Stock) <= 0);

            // Query product list
            var productVmQuery = productsQuery
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,

                    Name = p.Name,

                    Description = p.Description,

                    Brand = p.Brand,

                    CreatedAt = p.CreatedAt,

                    imageUrl = p.ProductImages
                        .OrderBy(i => i.SortOrder)
                        .Select(i => i.Url)
                        .ToList(),

                    Categories = new List<CategoryViewModel>
                    {
                new CategoryViewModel
                {
                    Id = p.Category.Id,
                    Name = p.Category.Name
                }
                    },

                    Variants = p.ProductVariants
                        .Select(v => new ProductVariantsViewModel
                        {
                            Id = v.Id,

                            Price = v.Price,

                            Stock = v.Stock
                        })
                        .ToList()
                });

            var totalCount = await productVmQuery.CountAsync();

            var items = await productVmQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var pagedProducts = new PaginatedList<ProductViewModel>(
                items,
                totalCount,
                page,
                pageSize
            );

            return new ProductManagementViewModel
            {
                TotalProducts = totalProducts,

                LowStockProducts = lowStockProducts,

                OutOfStockProducts = outOfStockProducts,

                TotalCategories = totalCategories,

                Products = pagedProducts
            };
        }
        public async Task<UserManagementViewModel> GetUserManagementAsync(int page, int pageSize)
        {
            var now = DateTime.Now;

            var usersQuery = _context.Users
                .AsNoTracking();

            var totalUsers = await usersQuery.CountAsync();

            var activeUsers = await usersQuery
                .CountAsync(u => u.IsActive);

            var lockedUsers = await usersQuery
                .CountAsync(u => u.LockoutUntil != null && u.LockoutUntil > now);

            var userVmQuery = usersQuery
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new UserItemViewModel
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    FullName = u.FullName,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    LockoutUntil = u.LockoutUntil,

                    IsLocked = u.LockoutUntil != null && u.LockoutUntil > now,

                    RoleName = u.UserRoles
                        .Select(ur => ur.Role.Name)
                        .FirstOrDefault() ?? "Customer"
                });

            var pagedUsers = await _pagingReponsitory.GetPagedDataAsync(
                userVmQuery,
                page,
                pageSize
            );

            return new UserManagementViewModel
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                LockedUsers = lockedUsers,
                Users = pagedUsers
            };
        }
        public async Task<CategoryManagementViewModel> GetCategoryManagementAsync(int page, int pageSize)
        {
            var categoriesQuery = _context.Categories
                .AsNoTracking();

            var totalCategories = await categoriesQuery.CountAsync();

            var categoryVmQuery = categoriesQuery
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    ParentId = c.ParentId,
                    CreatedAt = c.CreatedAt
                });

            var pagedCategories = await _pagingReponsitory.GetPagedDataAsync(
                categoryVmQuery,
                page,
                pageSize
            );

            return new CategoryManagementViewModel
            {
                TotalCategories = totalCategories,
                Categories = pagedCategories
            };
        }
    }
}
