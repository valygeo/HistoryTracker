using HistoryTracker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Domain.MetaData;
using HistoryTracker.Contexts;
using HistoryTracker.Gateways;

namespace HistoryTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _clocPath;

        public HomeController(IConfiguration configuration)
        {
            var clocPathConfig = configuration["AppSettings:ClocPath"];
            if (string.IsNullOrWhiteSpace(clocPathConfig))
            {
                throw new ArgumentNullException(nameof(clocPathConfig), "ClocPath configuration is missing or empty.");
            }
            _clocPath = Path.GetFullPath(clocPathConfig);
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
                new CreateAllTimeLogFileContext(new CreateAllTimeLogFileGateway()), new ExtractAllCommitsContext(new ExtractAllCommitsGateway()));
            var response = context.Execute(githubUrl);
            return Json(new { statistics = response.Statistics });
        }


        [HttpGet]
        [Route("{repositoryUrl:required}/get-complexity-metrics")]
        public IActionResult GetComplexityMetrics([FromRoute] string repositoryUrl)
        {
            var context = new MergeChangeFrequenciesAndNumberOfCodeLinesContext(
                new CloneRepositoryContext(new CloneRepositoryGateway()),
                new GenerateCsvWithChangeFrequenciesOfAllModulesContext(
                    new GenerateCsvWithChangeFrequenciesOfAllModulesGateway(), new CreateAllTimeLogFileContext(new CreateAllTimeLogFileGateway()), new ExtractCommitsContext(new ExtractAllCommitsGateway())),
                new GenerateCsvWithNumberOfCodeLinesContext(new GenerateCsvWithNumberOfCodeLinesGateway()),
                new MergeChangeFrequenciesAndNumberOfCodeLinesGateway());
            var response = context.Execute(repositoryUrl,_clocPath);
            return Json(response.MergedCsvFilePath);
        }

        [HttpGet("get-complexity-metrics-for-specific-period")]
        public IActionResult GetComplexityMetricsForSpecificPeriod([FromQuery] ComplexityMetricsRequest request)
        {
            var context = new MergeChangeFrequenciesAndNumberOfCodeLinesFromSpecificPeriodContext(
                new CloneRepositoryContext(new CloneRepositoryGateway()),
                new GenerateCsvWithChangeFrequenciesOfModulesFromSpecificPeriodContext(
                    new GenerateCsvWithChangeFrequenciesOfModulesFromSpecificPeriodGateway(),
                    new CreateAllTimeLogFileContext(new CreateAllTimeLogFileGateway()),
                    new ExtractCommitsForSpecifiedPeriodFromLogFileContext(
                        new ExtractCommitsForSpecifiedPeriodFromLogFileGateway())),
                new GenerateCsvWithNumberOfCodeLinesContext(new GenerateCsvWithNumberOfCodeLinesGateway()),
                new MergeChangeFrequenciesAndNumberOfCodeLinesFromSpecificPeriodGateway());
            var response = context.Execute(request, _clocPath);
            if(response.IsSuccess)
                return Json(response.MergedCsvFilePath);
            return Json(new { Error = response.Error});
        }

        [HttpGet("get-main-authors-per-modules-by-revisions")]
        public IActionResult GetMainAuthorsPerModule([FromQuery] ComplexityMetricsRequest request)
        {
            var context = new MergeMainAuthorsAndNumberOfCodeLinesFilesContext(
                new CloneRepositoryContext(new CloneRepositoryGateway()),
                new GenerateCsvWithMainAuthorsPerModuleContext(new GenerateCsvWithMainAuthorsPerModuleGateway(),
                    new CreateAllTimeLogFileContext(new CreateAllTimeLogFileGateway()),
                    new ExtractCommitsForSpecifiedPeriodFromLogFileContext(
                        new ExtractCommitsForSpecifiedPeriodFromLogFileGateway())),
                new GenerateCsvWithNumberOfCodeLinesContext(new GenerateCsvWithNumberOfCodeLinesGateway()),
                new MergeMainAuthorsAndNumberOfCodeLinesFilesGateway());
            var response = context.Execute(request, _clocPath);
            if (response.IsSuccess)
                return Json(response.MergedCsvFilePath);
            return Json(new { Error = response.Error });
        }

        [HttpGet]
        [Route("{repositoryUrl:required}/get-metrics-for-power-law")]
        public IActionResult GetMetricsForPowerLaw([FromRoute] string repositoryUrl)
        {
            var cloneRepositoryContext = new CloneRepositoryContext(new CloneRepositoryGateway());
            var cloneRepositoryResponse = cloneRepositoryContext.Execute(repositoryUrl);
            var context = new GenerateCsvWithChangeFrequenciesOfAllModulesContext(
                new GenerateCsvWithChangeFrequenciesOfAllModulesGateway(),
                new CreateAllTimeLogFileContext(new CreateAllTimeLogFileGateway()),
                new ExtractCommitsContext(new ExtractAllCommitsGateway()));
            var response = context.Execute(cloneRepositoryResponse.ClonedRepositoryPath);
            if (response.IsSuccess)
                return Json(response.GeneratedCsvPath);
            return Json(new { Error = response.Error });
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
