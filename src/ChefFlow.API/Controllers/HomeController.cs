using Microsoft.AspNetCore.Mvc;

namespace ChefFlow.API.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
