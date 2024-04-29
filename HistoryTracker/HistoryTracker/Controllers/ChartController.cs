using Microsoft.AspNetCore.Mvc;

namespace HistoryTracker.Controllers
{
    [Route("Chart")]
    public class ChartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
