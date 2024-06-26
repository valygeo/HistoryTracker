﻿using HistoryTracker.Contexts;
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
        [HttpGet("file-main-authors-per-files")]
        public JsonResult GetFileMainAuthorsPerFiles(string filePath)
        {
            var context = new GetFileMainAuthorsPerFileContext();
            var result = context.Execute(filePath);
            return Json(result.Hierarchy);
        }

        [HttpGet("power-law-change-frequencies-per-file")]
        public JsonResult GetChangeFrequenciesPerFile(string filePath)
        {
            var context = new DisplayPowerLawForModificationFrequenciesPerFileContext();
            var result = context.Execute(filePath);
            return Json(result.ChangeFrequenciesPerFile);
        }
    }
}
