﻿using HistoryTracker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using HistoryTracker.Gateways;
using HistoryTrackers.Contexts;

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
        public IActionResult GetSummaryData([FromRoute]string githubUrl, bool needToBeSaved)
        {
            var context = new GetSummaryDataContext(new GetSummaryDataGateway(), new CreateLogFileContext(new CreateLogFileGateway(), new CloneRepositoryContext(new CloneRepositoryGateway())));
            var response = context.Execute(githubUrl);
            return Json(new { fileContent = response.FileContent });
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
