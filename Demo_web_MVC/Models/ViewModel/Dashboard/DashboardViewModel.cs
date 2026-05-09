using Demo_web_MVC.Models.ViewModel.Oder;
using Demo_web_MVC.Models.ViewModel.Product;

namespace Demo_web_MVC.Models.ViewModel.Dashboard
{
    public class DashboardViewModel
    {
            public int orderId { get; set; }
            public List<OderViewModel> Orders { get; set; } = new List<OderViewModel>();
            public List<ProductViewModel> Products { get; set; } = new List<ProductViewModel>();
        
    }
}
