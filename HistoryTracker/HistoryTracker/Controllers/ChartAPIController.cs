using HistoryTracker.Contexts;
using Microsoft.AspNetCore.Mvc;

namespace HistoryTracker.Controllers
{
    [Route("chart-api-controller")]
    [Produces("application/json")]
    public class ChartAPIController : Controller
    {
        [HttpGet("hotspots-frequencies-and-complexity-for-all-time")]
        public JsonResult GetHierarchyData(string filePath)
        {
            var context = new GetHotspotsFrequenciesAndComplexityPerFileFromAllTimeContext();
            var result = context.Execute(filePath);
            return Json(result.Hierarchy);
        }

        [HttpGet("hotspots-frequencies-and-complexity-for-specific-period")]
        public JsonResult GetHotspotsFrequenciesAndComplexityForSpecificPeriod(string filePath)
        {
            var context = new GetHotspotsFrequenciesAndComplexityPerFileFromSpecificPeriodContext();
            var result = context.Execute(filePath);
            return Json(result.Hierarchy);
        }
    }
}
