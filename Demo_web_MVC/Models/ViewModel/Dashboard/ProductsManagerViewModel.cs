using Demo_web_MVC.Models.ViewModel.Product;

namespace Demo_web_MVC.Models.ViewModel.Dashboard
{
    public class ProductsManagerViewModel
    {
        public List<ProductViewModel> Products { get; set; }= new List<ProductViewModel>();
    }
}
