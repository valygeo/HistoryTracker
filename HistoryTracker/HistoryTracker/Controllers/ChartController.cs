
using HistoryTracker.Contexts;
using Microsoft.AspNetCore.Mvc;

namespace HistoryTracker.Controllers
{
    [Route("Chart")]
    [Produces("application/json")]
    public class ChartController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
