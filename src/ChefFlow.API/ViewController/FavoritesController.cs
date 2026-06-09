using Microsoft.AspNetCore.Mvc;

namespace ChefFlow.API.Controllers
{
    public class MyRecipesController : Controller
    {
        public IActionResult Index() => View();
        public IActionResult Details() => View();
    }
}