using Microsoft.AspNetCore.Mvc;

namespace ChefFlow.API.Controllers
{
    public class FavoritesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}