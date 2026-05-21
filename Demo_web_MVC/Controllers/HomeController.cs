using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel;
using Demo_web_MVC.Repository.Addresss;
using Demo_web_MVC.Service.Address;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace Demo_web_MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public readonly IAddressService _addressService;
        
        public HomeController(ILogger<HomeController> logger, IAddressService address)
        {
            _logger = logger;
            _addressService = address;
        }
        

        public  IActionResult Index()
        {
            //return RedirectToAction("Index", "Product");
            return View();
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
