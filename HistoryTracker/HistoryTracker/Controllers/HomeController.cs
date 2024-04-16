using HistoryTracker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using HistoryTracker.Contexts;
using HistoryTracker.Gateways;

namespace HistoryTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("{githubUrl:required}/get-summary-data")]
        public IActionResult GetSummaryData([FromRoute]string githubUrl)
        {
            var context = new GetSummaryDataContext(
                new CloneRepositoryContext(new CloneRepositoryGateway()),
                new CreateLogFileContext(new CreateLogFileGateway()), new ReadLogFileContext(new ReadLogFileGateway()), new ExtractAllCommitsContext());
            var response = context.Execute(githubUrl);
            return Json(new { statistics = response.Statistics });
        }

        [HttpGet]
        [Route("{githubUrl:required}/get-change-frequencies")]
        public IActionResult GetChangeFrequencies([FromRoute] string githubUrl)
        {
            var context = new GenerateCsvWithChangeFrequenciesOfModulesContext(new GenerateCsvWithChangeFrequenciesOfModulesGateway(),
                new CloneRepositoryContext(new CloneRepositoryGateway()),
                new CreateLogFileContext(new CreateLogFileGateway()), new ReadLogFileContext(new ReadLogFileGateway()),
                new ExtractAllCommitsContext());
            var response = context.Execute(githubUrl);
            return Json(new { changeFrequencies = response.Revisions });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
