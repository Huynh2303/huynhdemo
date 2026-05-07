using Demo_web_MVC.Models.ViewModel.Product;
using Demo_web_MVC.Repository;
using Demo_web_MVC.Repository.Product;
namespace Demo_web_MVC.Service.Product
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<ProductService> _logger;
        public ProductService(IProductRepository productRepository, ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }       
        public async Task<ProductViewModel> details(int id)    
        {
            
            return await _productRepository.DetailsAsnyc(id);
        }
        public async Task<ProductViewModel> creat(ProductViewModel product)
        {
            try
            {
                if (product == null || string.IsNullOrEmpty(product.Name) || product.CategoryId <= 0)
                {
                    throw new ArgumentException("Thông tin sản phẩm không hợp lệ.");
                }
                // Gọi phương thức AddAsnyc từ repository để thêm sản phẩm
                return await _productRepository.AddAsnyc(product) ;
            }
            catch (Exception ex)
            {
                // Ghi log lỗi chi tiết
                _logger.LogError(ex, "Lỗi khi tạo sản phẩm");
                _logger.LogError(ex.Message, ex);

                // Ném lại lỗi với thông báo chi tiết hơn
                throw new Exception($"Có lỗi khi tạo sản phẩm: {ex.Message}", ex);
            }
        }
        public async Task<ProductViewModel> update(int id, ProductViewModel product)
        {
            return await _productRepository.UpdateAsync(id, product);
        }
        public async Task<bool> delete(int id)
        {
            return await _productRepository.DeleteAsync(id);
        }
        public async Task<List<ProductViewModel>> getAll()
        {
            return await _productRepository.GetAllAsync();
        }
        public async Task<ProductViewModel> getbyid(int id)
        {
            return await _productRepository.GetByIdAsync(id);
        }
        public async Task<int?> GetProductIdByVariantIdAsync(int variantId)
        {
            return await _productRepository.GetProductIdByVariantIdAsync(variantId);
        }
    }
}