using Microsoft.AspNetCore.Mvc;

namespace ChefFlow.API.ViewController
{
    public class CabinetController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}