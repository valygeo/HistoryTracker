using HistoryTracker.Contexts;
using Microsoft.AspNetCore.Mvc;

namespace HistoryTracker.Controllers
{
    [Route("Chart")]
    public class ChartController : Controller
    {
        public IActionResult Index()
        {
            var context = new ExtractDataFromMergedCsvFileContext();
            context.Execute(
                "C:\\Users\\Vali\\Documents\\ClonedRepositories\\app-stagiatura-2023\\app-stagiatura-2023_complexity_metrics.csv");
            return View();
        }
    }
}
