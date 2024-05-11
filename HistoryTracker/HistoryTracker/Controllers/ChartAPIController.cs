using HistoryTracker.Contexts;
using Microsoft.AspNetCore.Mvc;

namespace HistoryTracker.Controllers
{
    [Route("chart-api-controller")]
    [Produces("application/json")]
    public class ChartAPIController : Controller
    {
        [HttpGet]
        public JsonResult GetHierarchyData()
        {
            var context = new ExtractDataFromMergedCsvFileContext();
            var result =
                context.Execute(
                    "C:\\Users\\Vali\\Documents\\ClonedRepositories\\app-stagiatura-2023\\app-stagiatura-2023_complexity_metrics.csv");
            return Json(result.Hierarchy);
        }
    }
}
