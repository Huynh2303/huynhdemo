using Demo_web_MVC.Models.ViewModel.Product;

namespace Demo_web_MVC.Service
{
    public interface IProductService
    {
        Task < ProductViewModel> creat (ProductViewModel product);
        Task < ProductViewModel> update (int id,ProductViewModel product);
        Task < bool> delete (int id);
        
        Task < ProductViewModel> details (int id);        
        Task< List<ProductViewModel>> getAll();
        Task<ProductViewModel> getbyid(int id);
        Task<int?> GetProductIdByVariantIdAsync(int variantId);
    }                   
}
