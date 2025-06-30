using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Npgsql;
using ReportService.Application.Services;
using System;
using System.Threading.Tasks;

namespace ReportService.Controllers
{
    [Route("api/[controller]")]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportController> _logger;
        public ReportController(
            IReportService reportService,
            ILogger<ReportController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        [HttpGet]
        [Route("{year}/{month}")]
        public async Task<IActionResult> Download(int year, int month)
        {
            if (year < 1900 || year > 2100)
            {
                _logger.LogWarning("Invalid year parameter: {Year}", year);
                return BadRequest(new { error = "Year must be between 1900 and 2100" });
            }

            if (month < 1 || month > 12)
            {
                _logger.LogWarning("Invalid month parameter: {Month}", month);
                return BadRequest(new { error = "Month must be between 1 and 12" });
            }

            try
            {
                var result = await _reportService.GenerateReportAsync(year, month);
                _logger.LogInformation("Report generated successfully for {Year}/{Month}", year, month);
                return result;
            }
            catch (Exception e)
            {
                // This should rarely happen now since the service handles most errors
                _logger.LogError(e, "Unexpected error in controller for {Year}/{Month}", year, month);
                return StatusCode(500, new { 
                    error = "An unexpected error occurred while processing your request"
                });
            }
        }
    }
}
