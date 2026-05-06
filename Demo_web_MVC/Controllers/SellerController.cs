using Microsoft.AspNetCore.Mvc;

namespace Demo_web_MVC.Controllers
{
    public class SellerController : Controller
    {
        // Dashboard action để trả về trang dashboard của người bán
        public IActionResult Dashboard()
        {
            // Trả về view Dashboard.cshtml cho Seller
            return View();
        }
    }
}
