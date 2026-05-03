using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Category;
using Demo_web_MVC.Models.ViewModel.Product;
using Demo_web_MVC.Models.ViewModel.Search;
using Demo_web_MVC.Repository.Paging;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
namespace Demo_web_MVC.Repository.Search
{  
    public class SearchReponsitory:ISearchReponsitory
    {
        private readonly AppDatabase _context;
        private readonly ILogger<SearchReponsitory> _logger;
        private readonly IPagingReponsitory _pagingReponsitory;
        public SearchReponsitory(AppDatabase context, ILogger<SearchReponsitory> logger,IPagingReponsitory pagingReponsitory)
        {
            _context = context;
            _logger = logger;
           _pagingReponsitory = pagingReponsitory;
        }
        public async Task<SearchViewModel> SearchAsync(string searchQuery, int? page = null, int pageSize = 10)
        {
            // Kiểm tra nếu searchQuery là null hoặc trống
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                return new SearchViewModel
                {
                    ErrorMessage = "Please enter a valid search query.",
                    SearchStatus = "Error"
                };
            }

            // Tạo query tìm kiếm sản phẩm từ cơ sở dữ liệu
            var query = _context.Products
                .Where(p => p.Name.Contains(searchQuery) || (p.Description ?? "").Contains(searchQuery))
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    imageUrl = p.ProductImages.Select(pi => pi.Url).ToList() ?? new List<string>(),
                    Variants = p.ProductVariants.Select(v => new ProductVariantsViewModel
                    {
                        Id = v.Id,
                        ProductId = v.ProductId,
                        Color = v.Color,
                        Price = v.Price,
                        Stock = v.Stock,
                        ImageUrlsVariants = v.ProductVariantImages.Select(pvi => pvi.Url).ToList() ?? new List<string>()
                    }).ToList()
                });

            // Nếu không yêu cầu phân trang, lấy tất cả kết quả
            if (!page.HasValue)
            {
                var products = await query.ToListAsync();  // Lấy toàn bộ kết quả tìm kiếm

                return new SearchViewModel
                {
                    SearchQuery = searchQuery,
                    ProductVMResults = products,
                    TotalResults = products.Count, // Tổng số kết quả tìm kiếm
                    SearchStatus = "Success", // Trạng thái tìm kiếm thành công
                    ErrorMessage = products.Any() ? null : "No results found for the search query." // Nếu không có kết quả, thông báo lỗi
                };
            }

            // Nếu yêu cầu phân trang, gọi phương thức phân trang
            var paginatedProducts = await _pagingReponsitory.GetPagedDataAsync(query, page.Value, pageSize);

            return new SearchViewModel
            {
                SearchQuery = searchQuery,
                ProductVMResults = paginatedProducts.Items,  // Dữ liệu phân trang
                TotalResults = paginatedProducts.TotalCount, // Tổng số kết quả
                SearchStatus = "Success",
                ErrorMessage = paginatedProducts.Items.Any() ? null : "No results found for the search query."
            };
        }

    }
}
