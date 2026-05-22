using Demo_web_MVC.Service.Admin;
using Microsoft.AspNetCore.Mvc;

namespace Demo_web_MVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminService _adminDashboardService;

        public AdminController(IAdminService adminDashboardService)
        {
            _adminDashboardService = adminDashboardService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Dashboard()
        {
            var model = await _adminDashboardService.GetAdminDashboardAsync();

            return PartialView("Dashboard", model);
        }
    }
}
