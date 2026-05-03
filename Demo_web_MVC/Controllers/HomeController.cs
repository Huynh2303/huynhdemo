using System.Diagnostics;
using Demo_web_MVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace Demo_web_MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        // xin chao 
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Index", "Product");
            //return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult TestSession()
        {
            HttpContext.Session.SetString("TestKey", "Hello Session SQL");

            return Content("Session saved");
        }
        public IActionResult ReadSession()
        {
            var value = HttpContext.Session.GetString("TestKey");

            return Content(value ?? "NULL");

        }
    }
}
