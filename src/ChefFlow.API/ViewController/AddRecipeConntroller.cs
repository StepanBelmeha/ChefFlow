using Microsoft.AspNetCore.Mvc;

namespace ChefFlow.API.Controllers
{
    public class AddRecipeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}