using HistoryTracker.Contexts;
using Microsoft.AspNetCore.Mvc;

namespace HistoryTracker.Controllers
{
    [Route("chart-api-controller")]
    [Produces("application/json")]
    public class ChartAPIController : Controller
    {
        [HttpGet]
        public JsonResult GetHierarchyData(string filePath)
        {
            var context = new GetHotspotsFrequenciesAndComplexityPerFileFromAllTimeContext();
            var result = context.Execute(filePath);
            return Json(result.Hierarchy);
        }

        [HttpGet]
        public JsonResult GetHierarchyDataFromSpecificPeriod(string filePath)
        {
            var context = new GetHotspotsFrequenciesAndComplexityPerFileFromSpecificPeriodContext();
            var result = context.Execute(filePath);
            return Json(result.Hierarchy);
        }
    }
}
