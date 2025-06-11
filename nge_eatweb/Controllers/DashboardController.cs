using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Mvc;

namespace nge_eatweb.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
