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
        public async Task<ProductViewModel?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Bắt đầu lấy thông tin sản phẩm. ProductId = {ProductId}", id);

            var product = await _context.Products
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    CategoryId = p.CategoryId,
                    Name = p.Name,
                    Description = p.Description,
                    Brand = p.Brand,
                    CreatedAt = p.CreatedAt,

                    // Ảnh sản phẩm
                    imageUrl = p.ProductImages
                        .OrderBy(img => img.SortOrder)
                        .Select(img => img.Url)
                        .ToList(),

                    // Biến thể sản phẩm
                    Variants = p.ProductVariants
                        .Select(v => new ProductVariantsViewModel
                        {
                            Id = v.Id,
                            ProductId = v.ProductId,
                            Size = v.Size,
                            Color = v.Color,
                            Price = v.Price,
                            Stock = v.Stock,

                            // Ảnh của biến thể
                            ImageUrlsVariants = v.ProductVariantImages
                                .OrderBy(img => img.SortOrder)
                                .Select(img => img.Url)
                                .ToList()
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                _logger.LogWarning("Không tìm thấy sản phẩm. ProductId = {ProductId}", id);
                return null;
            }

            _logger.LogInformation(
                "Lấy sản phẩm thành công. ProductId = {ProductId}, Name = {ProductName}, ImageCount = {ImageCount}, VariantCount = {VariantCount}",
                product.Id,
                product.Name,
                product.imageUrl?.Count ?? 0,
                product.Variants?.Count ?? 0
            );

            if (product.imageUrl != null && product.imageUrl.Any())
            {
                foreach (var img in product.imageUrl)
                {
                    _logger.LogInformation("Ảnh sản phẩm ProductId = {ProductId}: {ImageUrl}", product.Id, img);
                }
            }
            else
            {
                _logger.LogWarning("Sản phẩm không có ảnh. ProductId = {ProductId}", product.Id);
            }

            if (product.Variants != null && product.Variants.Any())
            {
                foreach (var variant in product.Variants)
                {
                    _logger.LogInformation(
                        "Variant: VariantId = {VariantId}, ProductId = {ProductId}, Size = {Size}, Color = {Color}, Price = {Price}, Stock = {Stock}, VariantImageCount = {VariantImageCount}",
                        variant.Id,
                        variant.ProductId,
                        variant.Size,
                        variant.Color,
                        variant.Price,
                        variant.Stock,
                        variant.ImageUrlsVariants?.Count ?? 0
                    );

                    if (variant.ImageUrlsVariants != null && variant.ImageUrlsVariants.Any())
                    {
                        foreach (var variantImg in variant.ImageUrlsVariants)
                        {
                            _logger.LogInformation(
                                "Ảnh variant: VariantId = {VariantId}, ImageUrl = {ImageUrl}",
                                variant.Id,
                                variantImg
                            );
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Variant không có ảnh. VariantId = {VariantId}", variant.Id);
                    }
                }
            }
            else
            {
                _logger.LogWarning("Sản phẩm không có biến thể. ProductId = {ProductId}", product.Id);
            }

            return product;
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
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                    .ThenInclude(v => v.ProductVariantImages)
                .AsSplitQuery()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                throw new Exception("Product not found");
            }

            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == model.CategoryId);

            if (!categoryExists)
            {
                throw new Exception("Category not found");
            }

            // 1. Update thông tin chính của sản phẩm
            product.CategoryId = model.CategoryId;
            product.Name = model.Name;
            product.Description = model.Description;
            product.Brand = model.Brand;

            // 2. Thêm ảnh mới cho sản phẩm nếu có
            // model.imageUrl là danh sách đường dẫn/tên ảnh đã được upload từ Controller
            if (model.imageUrl != null && model.imageUrl.Any())
            {
                foreach (var url in model.imageUrl)
                {
                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        product.ProductImages.Add(new ProductImage
                        {
                            Url = url,
                            SortOrder = 0
                        });
                    }
                }
            }

            var inputVariants = model.Variants ?? new List<ProductVariantsViewModel>();

            // 3. Update hoặc thêm mới variant
            foreach (var variantVm in inputVariants)
            {
                // Nếu Id > 0 thì đây là variant cũ
                var existingVariant = product.ProductVariants
                    .FirstOrDefault(v => v.Id == variantVm.Id && variantVm.Id > 0);

                if (existingVariant != null)
                {
                    // Update variant cũ
                    existingVariant.Size = variantVm.Size;
                    existingVariant.Color = variantVm.Color;
                    existingVariant.Price = variantVm.Price;
                    existingVariant.Stock = variantVm.Stock;

                    // Thêm ảnh mới cho variant cũ nếu có
                    if (variantVm.ImageUrlsVariants != null && variantVm.ImageUrlsVariants.Any())
                    {
                        foreach (var url in variantVm.ImageUrlsVariants)
                        {
                            if (!string.IsNullOrWhiteSpace(url))
                            {
                                existingVariant.ProductVariantImages.Add(new ProductVariantImage
                                {
                                    Url = url,
                                    SortOrder = 0
                                });
                            }
                        }
                    }
                }
                else
                {
                    // Thêm variant mới
                    var newVariant = new ProductVariant
                    {
                        ProductId = product.Id,
                        Size = variantVm.Size,
                        Color = variantVm.Color,
                        Price = variantVm.Price,
                        Stock = variantVm.Stock,
                        ProductVariantImages = new List<ProductVariantImage>()
                    };

                    // Thêm ảnh cho variant mới nếu có
                    if (variantVm.ImageUrlsVariants != null && variantVm.ImageUrlsVariants.Any())
                    {
                        foreach (var url in variantVm.ImageUrlsVariants)
                        {
                            if (!string.IsNullOrWhiteSpace(url))
                            {
                                newVariant.ProductVariantImages.Add(new ProductVariantImage
                                {
                                    Url = url,
                                    SortOrder = 0
                                });
                            }
                        }
                    }

                    product.ProductVariants.Add(newVariant);
                }
            }

            await _context.SaveChangesAsync();

            return new ProductViewModel
            {
                Id = product.Id,
                CategoryId = product.CategoryId,
                Name = product.Name,
                Description = product.Description,
                Brand = product.Brand,
                imageUrl = product.ProductImages.Select(img => img.Url).ToList(),

                Variants = product.ProductVariants.Select(v => new ProductVariantsViewModel
                {
                    Id = v.Id,
                    ProductId = v.ProductId,
                    Size = v.Size,
                    Color = v.Color,
                    Price = v.Price,
                    Stock = v.Stock,
                    ImageUrlsVariants = v.ProductVariantImages.Select(img => img.Url).ToList()
                }).ToList()
            };
        }
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (product == null)
                {
                    _logger.LogWarning("Không tìm thấy sản phẩm với id {ProductId}", id);
                    return false;
                }

                product.IsDeleted = true;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Đã xóa mềm sản phẩm id {ProductId}", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa mềm sản phẩm với id {ProductId}", id);
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