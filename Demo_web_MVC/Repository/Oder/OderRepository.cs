using Demo_web_MVC.Controllers;
using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Carts;
using Demo_web_MVC.Models.ViewModel.Oder;
using Demo_web_MVC.Repository.Addresss;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;


namespace Demo_web_MVC.Repository.Oder
{
    public class OderRepository: IOderRepository
    {
        public readonly AppDatabase _context;
        public readonly ILogger<OderRepository> _logger;  
        public readonly IAddressRepository _addressRepository;
        public OderRepository(AppDatabase context, ILogger<OderRepository> logger, IAddressRepository addressRepository)
        {
            _context = context;
            _logger = logger;
            _addressRepository = addressRepository;
        }
        public async Task<int> CreateOrderFromCartAsync(int userId, string paymentMethod)
        {
                        
            var cart = await _context.Carts
                             .Where(c => c.UserId == userId && c.Status == "active")
                             .Include(c => c.CartItems)
                             .FirstOrDefaultAsync();
            if ( cart == null || cart.CartItems == null || !cart.CartItems.Any())
            {
                _logger.LogWarning("No active cart found for user {UserId}", userId);
                throw new InvalidOperationException("No active cart found for the user.");
            }
            if (Enum.TryParse(paymentMethod, out PaymentMethod method))
            {
                // Tiến hành tạo đơn hàng nếu parsing thành công
                var order = new Order
                {
                    UserId = userId,
                    TotalAmount = cart.CartItems.Sum(ci => ci.Quantity * ci.Variant.Price),
                    Status = 0,
                    PaymentMethod = method,  // Gán giá trị enum vào PaymentMethod
                    CreatedAt = DateTime.Now
                };

                // Lưu đơn hàng vào cơ sở dữ liệu
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Lưu các mục trong đơn hàng
                foreach (var item in cart.CartItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        VariantId = item.VariantId,
                        Quantity = item.Quantity,
                        Price = item.Variant.Price
                    };
                    _context.OrderItems.Add(orderItem);
                }

                await _context.SaveChangesAsync(); // Lưu các mục trong đơn hàng

                // Trả về ID của đơn hàng vừa tạo
                return order.Id;
            }   
            else
            {
                // Trường hợp parsing thất bại (chẳng hạn "COD" không đúng)
                return -1;  // Hoặc xử lý lỗi khác
            }

        }
        public async Task<Order> GetOrderByIdAsync(  int orderId)
        {
            var order = await _context.Orders
         .Where(o => o.Id == orderId)
         .Include(o => o.OrderItems)  // Đảm bảo rằng các sản phẩm trong đơn hàng được bao gồm
         .FirstOrDefaultAsync();
            if (order == null)
            {
                _logger.LogError("đơn hàng không hợp lệ");
                throw new InvalidOperationException("Đơn hàng không hợp lệ");
            }
            var result = new Order
            {
                Id = orderId,
                UserId = order.UserId,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                CreatedAt = DateTime.Now,
                PaymentMethod = order.PaymentMethod,
            };
            return result;
        }
        public async Task<OderViewModel?> GetOrderDetailAsync( int userId,int orderId)
        {
            var result = await _context.Orders.AsNoTracking()
               .Where(o => o.Id == orderId && o.UserId == userId)
               .Select(o => new OderViewModel
               {
                   Id = o.Id,
                   TotalAmount = o.TotalAmount,
                   Status = o.Status,

                   Items = o.OrderItems.Select(item => new OderItemViewModel
                   {
                       Name = item.Variant.Product.Name,
                       Price = item.Price,
                       Quantity = item.Quantity,

                       Img = item.Variant.ProductVariantImages
                           .OrderBy(img => img.SortOrder)
                           .Select(img => img.Url)
                           .FirstOrDefault()
                   }).ToList()
               })
               .FirstOrDefaultAsync();

            if (result == null)
            {
                _logger.LogError("Không có order. userId={UserId}, orderId={OrderId}", userId, orderId);
                return null;
            }

            return result;
        }
        public async Task<List<OderViewModel>> GetOrdersByUserAsync(int userId)
        {
            var result = await _context.Orders.AsNoTracking()
               .Where(o =>  o.UserId == userId)
               .OrderByDescending(o => o.CreatedAt)
               .Select(o => new OderViewModel
               {
                   Id = o.Id,
                   TotalAmount = o.TotalAmount,
                   Status = o.Status,
                   CreateAt = o.CreatedAt,
                   Items = o.OrderItems.Select(item => new OderItemViewModel
                   {
                       Name = item.Variant.Product.Name,
                       Price = item.Price,
                       Quantity = item.Quantity,
                   }).ToList()
               }).ToListAsync();

            if (result.Count == 0)
            {
                _logger.LogInformation("User chưa có đơn hàng. userId={UserId}", userId);
            }

            return result;
        }
        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            if (!Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
            {
                return false;
            }

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return false;

            order.Status = parsedStatus;

            await _context.SaveChangesAsync();
            return true;
        }

    }
}
