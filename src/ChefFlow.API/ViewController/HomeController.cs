using Microsoft.AspNetCore.Mvc;

namespace ChefFlow.API.ViewController
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
