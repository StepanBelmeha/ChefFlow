using Microsoft.AspNetCore.Mvc;

namespace ChefFlow.API.Controllers
{
    public class CabinetController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}