using Demo_web_MVC.Controllers;
using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Address;
using Demo_web_MVC.Models.ViewModel.Carts;
using Demo_web_MVC.Models.ViewModel.Oder;
using Demo_web_MVC.Repository.Addresss;
using MailKit.Search;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<int> CreateOrderFromCartAsync(int userId, string paymentMethod, List<int> selectedCartItemIds)
        {
            var cart = await _context.Carts
                .Where(c => c.UserId == userId && c.Status == "active")
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Variant)
                .FirstOrDefaultAsync();

            if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
            {
                _logger.LogWarning("No active cart found for user {UserId}", userId);
                throw new InvalidOperationException("No active cart found for the user.");
            }

            var selectedItems = cart.CartItems.Where(ci => selectedCartItemIds.Contains(ci.Id)).ToList();

            if (selectedItems.Count == 0)
            {
                _logger.LogError("k co san pham nào");
                throw new InvalidOperationException("No selected items to checkout.");
            }

            // Tiến hành tính tổng số tiền và tạo đơn hàng
            var totalAmount = selectedItems
                .Where(ci => ci.Variant != null && ci.Variant.Price > 0)
                .Sum(ci => ci.Quantity * ci.Variant.Price);

            var order = new Order
            {
                UserId = userId,
                TotalAmount = totalAmount,
                Status = 0, // Trạng thái đơn hàng (0 = Chờ xác nhận)
                PaymentMethod = Enum.TryParse(paymentMethod, out PaymentMethod method) ? method : PaymentMethod.COD,
                CreatedAt = DateTime.Now
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Lưu các mục trong đơn hàng
            foreach (var item in selectedItems)
            {
                if (item.Variant == null)
                {
                    _logger.LogWarning("Item with VariantId {VariantId} has no variant data, skipping.", item.VariantId);
                    continue;
                }

                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    VariantId = item.VariantId,
                    Quantity = item.Quantity,
                    Price = item.Variant.Price
                };

                _context.OrderItems.Add(orderItem);
            }
            var orderLog = new OrderLog
            {
                OrderId = order.Id,
                PreviousStatus = null,
                Status = "Pending",
                ChangeType = "CREATE_ORDER",
                ActionBy = userId.ToString(),
                Reason = "User created order from cart",
                AdditionalInfo = $"PaymentMethod: {order.PaymentMethod}, TotalAmount: {order.TotalAmount}",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _context.OrderLogs.Add(orderLog);
            await _context.SaveChangesAsync();
            
            return order.Id;
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
                       Variant =item.Variant,
                       Img = item.Variant.ProductVariantImages
                           .OrderBy(img => img.SortOrder)
                           .Select(img => img.Url)
                           .FirstOrDefault()
                   }).ToList(),
                    AddressViewModels = o.User.Addresses.Select(address => new AddressViewModel
                    {
                        RecipientName = address.RecipientName,
                        PhoneNumber = address.PhoneNumber,
                        AddressLine = address.AddressLine,
                        City = address.City,
                        Country = address.Country
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
            var previousStatus = order.Status;

            order.Status = parsedStatus;
            var orderLog = new OrderLog
            {
                OrderId = order.Id,
                PreviousStatus = previousStatus.ToString(),
                Status = parsedStatus.ToString(),
                ChangeType = GetOrderChangeType(parsedStatus),
                ActionBy = "System",
                Reason = GetOrderReason(parsedStatus),
                AdditionalInfo = null,
                CreatedAt = DateTime.Now
            };
            _context.OrderLogs.Add(orderLog);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> CancelOrderAsync(int orderId, int userId)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            {
                _logger.LogWarning("Không tìm thấy order. orderId={OrderId}, userId={UserId}", orderId, userId);
                return false;
            }


            if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled)
            {
                _logger.LogWarning("Không thể huỷ đơn. status={Status}", order.Status);
                return false;
            }
            var previousStatus = order.Status;
            order.Status = OrderStatus.Cancelled;
            var orderLog = new OrderLog
            {
                OrderId = order.Id,
                PreviousStatus = previousStatus.ToString(),
                Status = order.Status.ToString(),
                ChangeType = "CANCEL_ORDER",
                ActionBy = userId.ToString(),
                Reason = "User cancelled order",
                AdditionalInfo = null,
                CreatedAt = DateTime.Now
            };
            _context.OrderLogs.Add(orderLog);
            await _context.SaveChangesAsync();

            return true;

        }
        public async Task<decimal> CalculateOrderTotalAsync(int userId)
        {
            var result = await _context.Orders.Where(o => o.UserId == userId).SumAsync(o => o.TotalAmount);
            return result;

        }
        public async Task<CheckoutViewModel> CheckOutAsync(int userId,List<int> selectedCartItemIds)
        {
            // Lấy các sản phẩm trong giỏ hàng
            var cartItems = await _context.CartItems
                .Where(ci => selectedCartItemIds.Contains(ci.Id)&& ci.Cart.UserId == userId && ci.Cart.Status == "active")
                .Include(ci => ci.Variant)
                .ThenInclude(ci => ci.ProductVariantImages) 
                .Include(ci => ci.Variant.Product)   
                .ToListAsync();
            if (cartItems.Count > 0)
                _logger.LogInformation("có sản phẩm");
             
            var addressViewModels = await _context.Addresses
                .Where(a => a.UserId == userId)
                .Select(a => new AddressViewModel
                {
                    Id = a.Id,
                    RecipientName = a.RecipientName,
                    PhoneNumber = a.PhoneNumber,
                    AddressLine = a.AddressLine,
                    City = a.City,
                    Country = a.Country,
                    IsDefault = a.IsDefault
                })
                .ToListAsync();

            
            var totalAmount = cartItems.Sum(ci => ci.Quantity * ci.Variant.Price);

            // Tạo model CheckoutViewModel
            var model = new CheckoutViewModel
            {
                CartItems = cartItems.Select(ci => new CartItemViewModel
                {
                    Id = ci.Id,
                    ProductName = ci.Variant.Product.Name,
                    Price = ci.Variant.Price,
                    Quantity = ci.Quantity,
                    ImageUrl = ci.Variant.ProductVariantImages.FirstOrDefault()?.Url  
                }).ToList(),
                AddressViewModels = addressViewModels,
                TotalAmount = totalAmount,
                SelectedAddressId = addressViewModels.FirstOrDefault(a => a.IsDefault)?.Id,   
                PaymentMethod = PaymentMethod.COD
            };
            return model;
        }
        public async Task RemoveCartItemsAsync(List<int> selectedCartItemIds, int userId)
        {
            // Lấy giỏ hàng của người dùng với trạng thái "active"
            var cart = await _context.Carts
                .Where(c => c.UserId == userId && c.Status == "active")
                .Include(c => c.CartItems) // Bao gồm các CartItems của giỏ hàng
                .FirstOrDefaultAsync();

            if (cart != null)
            {
                // Lọc các CartItem mà ID có trong selectedCartItemIds
                var selectedItems = cart.CartItems.Where(ci => selectedCartItemIds.Contains(ci.Id)).ToList();

                // Xóa chỉ các CartItem đã chọn
                _context.CartItems.RemoveRange(selectedItems);
                await _context.SaveChangesAsync(); // Lưu thay đổi vào database
            }
        }
        public async Task<List<OderViewModel>> GetAllOrderIdsAsync(int orderId) 
        {
            
            var orderIds = await _context.Orders
                .AsNoTracking()

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


            return orderIds;
        }
        public async Task<List<OderViewModel>> GetAllOrders(int userId, string status)
        {
            IQueryable<Order> ordersQuery = _context.Orders.Where(o => o.UserId == userId);

            if (status != "All")
            {
                ordersQuery = ordersQuery.Where(o => o.Status == (OrderStatus)Enum.Parse(typeof(OrderStatus), status));
            }
            var orders = await ordersQuery
                .AsNoTracking()
                
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

            return orders;
        }
        //người bán
        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            
            var order = await _context.Orders
                                      .Where(o => o.Id == orderId)
                                      .FirstOrDefaultAsync();

            
            if (order == null)
            {
                _logger.LogWarning("Không tìm thấy đơn hàng với orderId={OrderId}", orderId);
                return false;  
            }

            
            var status = OrderStatus.Cancelled.ToString();

            
            var update = await UpdateOrderStatusAsync(orderId, status);

            if (update)  
            {
                _logger.LogInformation("Đơn hàng với orderId={OrderId} đã bị hủy thành công.", orderId);
                return true;
            }

            _logger.LogWarning("Không thể cập nhật trạng thái đơn hàng orderId={OrderId} thành Cancelled", orderId);
            return false;
        }
        public async Task<bool> CreateAsync(int orderId)
        {
           
            var order = await _context.Orders
                                      .Where(o => o.Id == orderId)
                                      .FirstOrDefaultAsync();

           
            if (order == null)
            {
                _logger.LogError("Không tìm thấy đơn hàng với orderId={OrderId}", orderId);
                return false;
            }

            
            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
            {
                _logger.LogWarning("Không thể nhận đơn. Đơn hàng không ở trạng thái hợp lệ (Pending/Confirmed). orderId={OrderId}, Status={Status}", orderId, order.Status);
                return false;
            }
            var previousStatus = order.Status;

            
            order.Status = OrderStatus.Shipping;

            var orderLog = new OrderLog
            {
                OrderId = order.Id,
                PreviousStatus = previousStatus.ToString(),
                Status = order.Status.ToString(),
                ChangeType = "SHIPPING_ORDER",
                ActionBy = "System",
                Reason = "Order shipping",
                AdditionalInfo = null,
                CreatedAt = DateTime.Now
            };

            _context.OrderLogs.Add(orderLog);

            await _context.SaveChangesAsync();

            
            _logger.LogInformation("Đơn hàng với orderId={OrderId} đã được chuyển sang trạng thái 'Shipping'.", orderId);

            return true;
        }
        private string GetOrderReason(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Order created",
                OrderStatus.Confirmed => "Order confirmed",
                OrderStatus.Shipping => "Order shipping",
                OrderStatus.Completed => "Order completed",
                OrderStatus.Cancelled => "Order cancelled",
                _ => "Order status updated"
            };
        }
        private string GetOrderChangeType(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "CREATE_ORDER",
                OrderStatus.Confirmed => "CONFIRM_ORDER",
                OrderStatus.Shipping => "SHIPPING_ORDER",
                OrderStatus.Completed => "COMPLETE_ORDER",
                OrderStatus.Cancelled => "CANCEL_ORDER",
                _ => "UPDATE_STATUS"
            };
        }
    }
}
