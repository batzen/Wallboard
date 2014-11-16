using Microsoft.AspNet.Mvc;

namespace Batzendev.Wallboard.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}