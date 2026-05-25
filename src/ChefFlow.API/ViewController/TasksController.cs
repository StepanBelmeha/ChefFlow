using Microsoft.AspNetCore.Mvc;

namespace ChefFlow.API.ViewController
{
    public class TasksController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}