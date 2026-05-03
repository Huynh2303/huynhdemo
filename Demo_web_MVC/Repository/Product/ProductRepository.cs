using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel;
using Demo_web_MVC.Models.ViewModel.Category;
using Demo_web_MVC.Models.ViewModel.Product;
using Microsoft.EntityFrameworkCore;

namespace Demo_web_MVC.Repository.Product
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDatabase _context;
        public readonly ILogger<ProductRepository> _logger;
        public ProductRepository(AppDatabase context, ILogger<ProductRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<ProductViewModel>> GetAllAsync()
        {
            var products = await _context.Products.AsNoTracking().AsSplitQuery().Select(p => new ProductViewModel
            {
                Id = p.Id,
                CategoryId = p.CategoryId,
                Name = p.Name,
                Description = p.Description,
                Brand = p.Brand,
                CreatedAt = p.CreatedAt,
                imageUrl = p.ProductImages.Select(pi => pi.Url).ToList() ?? new List<string>(),
                Variants = p.ProductVariants.Select(v => new ProductVariantsViewModel
                {
                    Price = v.Price,
                    Stock = v.Stock,
                }).ToList() 
            }).ToListAsync();
            return products;
        }
        public async Task<ProductViewModel> GetByIdAsync(int id)
        {
            var product = await _context.Products.AsNoTracking().Where(p => p.Id == id).Select(p => new ProductViewModel
            {
                Id = p.Id,
                CategoryId = p.CategoryId,
                Name = p.Name,
                Description = p.Description,
                Brand = p.Brand,
                CreatedAt = p.CreatedAt

            }).FirstOrDefaultAsync();
            return product!;
        }
        public async Task <ProductViewModel> DetailsAsnyc (int id)
        {
            var product = await _context.Products.AsNoTracking().Where(p => p.Id == id).Select(p => new ProductViewModel
            {
                Id = p.Id,
                CategoryId = p.CategoryId,
                Name = p.Name,
                Description = p.Description,
                Brand = p.Brand,
                CreatedAt = p.CreatedAt,
                imageUrl= p.ProductImages.Select(pi => pi.Url).ToList() ?? new List<string>(),
                Variants = p.ProductVariants
                .Select(v => new ProductVariantsViewModel
                {
                    Id = v.Id,
                    ProductId = v.ProductId,
                    Size = v.Size,
                    Color = v.Color,
                    Price = v.Price,
                    Stock = v.Stock,
                    ImageUrlsVariants = v.ProductVariantImages.Select(pvi => pvi.Url).ToList() ?? new List<string>(),
                })
                .ToList()
            }).FirstOrDefaultAsync();
            return product!;
        }
        public async Task<ProductViewModel> AddAsnyc(ProductViewModel product)
            {
                var newProduct = new Models.Product
                {
                    CategoryId = product.CategoryId,
                    Name = product.Name,    
                    Description = product.Description,
                    Brand = product.Brand,
                    CreatedAt = DateTime.Now,
                    ProductImages = product.imageUrl?.Select(url => new ProductImage
                    {
                        Url = $"/uploads/products/{url.Trim()}"  // Thêm tiền tố vào mỗi URL
                    }).ToList() ?? new List<ProductImage>(),
                    ProductVariants = product.Variants?.Select(v => new ProductVariant
                    {
                        Size = v.Size,
                        Color = v.Color,
                        Price = v.Price,
                        Stock = v.Stock,
                        ProductVariantImages = v.ImageUrlsVariants?.Select(url => new ProductVariantImage { Url = url }).ToList() ?? new List<ProductVariantImage>()
                    }).ToList() ?? new List<ProductVariant>()
                };
            
                _context.Products.Add(newProduct);
                await _context.SaveChangesAsync();
                product.Id = newProduct.Id;
                return product;
            }
        public async Task<ProductViewModel> UpdateAsync(int id, ProductViewModel model)
        {
            var product = await _context.Products
                .Include(p => p.ProductVariants)
                .ThenInclude(v => v.ProductVariantImages)
                .AsSplitQuery()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                throw new Exception("Product not found");

            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == model.CategoryId);

            if (!categoryExists)
                throw new Exception("Category not found");

            // Update thông tin product
            product.CategoryId = model.CategoryId;
            product.Name = model.Name;
            product.Description = model.Description;
            product.Brand = model.Brand;

            var inputVariants = model.Variants ?? new List<ProductVariantsViewModel>();

            // Các variant cần xóa
            var toDelete = product.ProductVariants
                .Where(v => !inputVariants.Any(iv => iv.Id == v.Id))
                .ToList();

            foreach (var variant in toDelete)
            {
                // Nếu có ảnh biến thể thì xóa trước
                if (variant.ProductVariantImages != null && variant.ProductVariantImages.Any())
                {
                    _context.ProductVariantImages.RemoveRange(variant.ProductVariantImages);
                }

                _context.ProductVariants.Remove(variant);
            }

            // Update hoặc Add
            foreach (var variantVm in inputVariants)
            {
                var existing = product.ProductVariants
                    .FirstOrDefault(v => v.Id == variantVm.Id);

                if (existing != null)
                {
                    existing.Size = variantVm.Size;
                    existing.Color = variantVm.Color;
                    existing.Price = variantVm.Price;
                    existing.Stock = variantVm.Stock;
                }
                else
                {
                    product.ProductVariants.Add(new ProductVariant
                    {
                        Size = variantVm.Size,
                        Color = variantVm.Color,
                        Price = variantVm.Price,
                        Stock = variantVm.Stock
                    });
                }
            }

            await _context.SaveChangesAsync();

            return new ProductViewModel
            {
                Id = product.Id,
                CategoryId = product.CategoryId,
                Name = product.Name,
                Description = product.Description,
                Brand = product.Brand
            };
        }
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var product = await _context.Products
                .Include(p => p.ProductVariants).Include(p => p.ProductImages).Include(p=>p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                   return false;
                if (product.ProductImages != null && product.ProductImages.Any())
                {
                    _context.ProductImages.RemoveRange(product.ProductImages);
                }
                
                // Xóa variants trước (explicit)
                _context.ProductVariants.RemoveRange(product.ProductVariants);

                // Sau đó xóa product
                _context.Products.Remove(product);

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                
                // Log lỗi nếu cần
                Console.WriteLine($"Error deleting product: {ex.Message}");
                _logger.LogError(ex, "Error deleting product with id {ProductId}", id);
                return false;
            }
        }
        public async Task<int?> GetProductIdByVariantIdAsync(int variantId)
        {
            return await _context.ProductVariants
                .Where(v => v.Id == variantId)
                .Select(v => (int?)v.ProductId)
                .FirstOrDefaultAsync();
        }
    }
}