using Microsoft.AspNetCore.Mvc;

namespace ChefFlow.API.Controllers
{
    public class AllRecipesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}