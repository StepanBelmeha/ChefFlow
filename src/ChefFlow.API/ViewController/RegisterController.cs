using Microsoft.AspNetCore.Mvc;

namespace ChefFlow.API.ViewController
{
    public class RegisterController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}