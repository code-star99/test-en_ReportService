using System;
using Microsoft.AspNetCore.Mvc;

namespace ReportService.Controllers
{
    [Route("api/[controller]")]
    public class TestController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { 
                message = "ReportService is running successfully!", 
                timestamp = DateTime.Now,
                version = "1.0.0",
                endpoints = new[] {
                    "/api/test - This test endpoint",
                    "/api/report/{year}/{month} - Generate report for specific year/month"
                }
            });
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { 
                status = "healthy", 
                service = "ReportService",
                timestamp = DateTime.Now
            });
        }
    }
} 