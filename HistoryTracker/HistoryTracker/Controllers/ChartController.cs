
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

        [HttpGet("hotspots-frequency-and-complexity-for-specific-period")]
        public IActionResult HotspotsFrequencyAndComplexityForSpecificPeriod()
        {
            return View();
        }
    }
}
