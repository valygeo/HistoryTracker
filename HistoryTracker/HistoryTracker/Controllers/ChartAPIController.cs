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
            var context = new GetHotspotsFrequenciesAndComplexityPerFileContext();
            var result = context.Execute(filePath);
            return Json(result.Hierarchy);
        }
    }
}
