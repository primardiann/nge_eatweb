using Microsoft.AspNetCore.Mvc;

namespace nge_eatweb.Controllers
{
    public class GoFoodController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
