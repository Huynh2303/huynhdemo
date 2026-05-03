using Demo_web_MVC.Models.ViewModel.Product;
using NuGet.DependencyResolver;

namespace Demo_web_MVC.Repository
{
    public interface IProductRepository
    {
        Task<List<Models.ViewModel.Product.ProductViewModel>> GetAllAsync();
                              
        Task<Models.ViewModel.Product.ProductViewModel> GetByIdAsync(int id);
        Task<ProductViewModel> AddAsnyc (ProductViewModel product);
        Task<ProductViewModel> UpdateAsync(int id,ProductViewModel product);
        Task<bool> DeleteAsync(int id);
        Task<ProductViewModel> DetailsAsnyc(int id);
        Task<int?> GetProductIdByVariantIdAsync(int variantId);

    }
}
