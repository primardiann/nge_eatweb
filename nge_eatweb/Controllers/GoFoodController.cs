using Microsoft.AspNetCore.Mvc;

namespace nge_eatweb.Controllers
{
    public class GofoodController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
