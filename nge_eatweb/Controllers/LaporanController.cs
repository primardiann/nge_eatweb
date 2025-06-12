using Microsoft.AspNetCore.Mvc;

namespace nge_eatweb.Controllers
{
    public class LaporanController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
