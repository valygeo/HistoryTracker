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
            var context = new GetSummaryStatisticsContext(
                new CloneRepositoryContext(new CloneRepositoryGateway()),
                new CreateLogFileContext(new CreateLogFileGateway()), new ReadLogFileContext(new ReadLogFileGateway()), new ExtractAllCommitsContext());
            var response = context.Execute(githubUrl);
            return Json(new { statistics = response.Statistics });
        }

        //[HttpGet]
        //[Route("{githubUrl:required}/get-change-frequencies")]
        //public IActionResult GetChangeFrequencies([FromRoute] string githubUrl)
        //{
        //    var context = new GenerateCsvWithChangeFrequenciesOfModulesContext(new GenerateCsvWithChangeFrequenciesOfModulesGateway(),
        //        new CloneRepositoryContext(new CloneRepositoryGateway()),
        //        new CreateLogFileContext(new CreateLogFileGateway()), new ReadLogFileContext(new ReadLogFileGateway()),
        //        new ExtractAllCommitsContext());
        //    var response = context.Execute(githubUrl);
        //    return Json(new { changeFrequencies = response.Revisions });
        //}

        //[HttpGet]
        //[Route("{githubUrl:required}/get-lines-of-code")]
        //public IActionResult GetLinesOfCode([FromRoute] string githubUrl)
        //{
        //    var context = new GenerateCsvWithNumberOfCodeLinesContext(new GenerateCsvWithNumberOfCodeLinesGateway(), new CloneRepositoryContext(new CloneRepositoryGateway()));
        //    var response = context.Execute(githubUrl);
        //    var context2 =
        //        new MergeChangeFrequenciesAndNumberOfCodeLinesContext(
        //            new MergeChangeFrequenciesAndNumberOfCodeLinesGateway());
        //    context2.Execute("C:\\Users\\Vali\\Documents\\ClonedRepositories\\app-stagiatura-2023\\app-stagiatura-2023_change_frequencies_of_modules.csv", "C:\\Users\\Vali\\Documents\\ClonedRepositories\\app-stagiatura-2023\\app-stagiatura-2023_counting_lines.csv");
        //    return Json("");
        //}


        [HttpGet]
        [Route("{repositoryUrl:required}/get-complexity-metrics")]
        public IActionResult GetComplexityMetrics([FromRoute] string repositoryUrl)
        {
            var context = new MergeChangeFrequenciesAndNumberOfCodeLinesContext(
                new CloneRepositoryContext(new CloneRepositoryGateway()),
                new GenerateCsvWithChangeFrequenciesOfModulesContext(
                    new GenerateCsvWithChangeFrequenciesOfModulesGateway(), new CreateLogFileContext(new CreateLogFileGateway()), new ReadLogFileContext(new ReadLogFileGateway()), new ExtractAllCommitsContext()),
                new GenerateCsvWithNumberOfCodeLinesContext(new GenerateCsvWithNumberOfCodeLinesGateway()),
                new MergeChangeFrequenciesAndNumberOfCodeLinesGateway());
            var response = context.Execute(repositoryUrl);
            return Json("");
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
