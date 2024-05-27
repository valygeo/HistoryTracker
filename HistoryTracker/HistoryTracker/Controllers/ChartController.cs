
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
        [HttpGet("file-main-authors-per-files")]
        public IActionResult FileMainAuthorsPerFiles()
        {
            return View();
        }

        [HttpGet("power-law-change-frequencies-per-file")]
        public IActionResult DisplayPowerLawWithChangeFrequenciesPerFile()
        {
            return View();
        }
    }
}
