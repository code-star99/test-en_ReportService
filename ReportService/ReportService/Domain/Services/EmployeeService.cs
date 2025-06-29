
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ReportService.Domain.Entities;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ReportService.Domain.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(HttpClient httpClient, IConfiguration configuration, ILogger<EmployeeService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<decimal> CalculateSalaryAsync(Employee employee) 
        {
            try
            {
                if (string.IsNullOrEmpty(employee?.Inn) || string.IsNullOrEmpty(employee?.EmployeeCode))
                {
                    _logger.LogWarning("Employee's Inn or EmployeeCode is null");
                    return 0;
                }

                var baseUrl = _configuration["ExternalServices:SalaryService:BaseUrl"];
                var res = await _httpClient.GetStringAsync($"{baseUrl}{employee.Inn}");

                if (decimal.TryParse(res, out var salary)) 
                {
                    _logger.LogInformation("Salary of {EmployeeName}: {Salary}", employee.Name, salary);
                    return salary;
                }

                _logger.LogWarning("Failed to parse salary for {EmployeeName}", employee.Name);
                return 0;
            }
            catch(Exception e)
            {
                _logger.LogWarning(e, "Failed to calculate salary for employee {EmployeeName}", employee?.Name);
                return 0;
            }

        }

        public async Task<string> GetEmployeeCodeAsync(string inn)
        {
            try
            {
                if (string.IsNullOrEmpty(inn))
                {
                    _logger.LogWarning("INN is empty");
                    return "DEFAULT";
                }

                var baseUrl = _configuration["ExternalServices:HrService:BaseUrl"];
                var response = await _httpClient.GetStringAsync($"{baseUrl}{inn}");

                _logger.LogInformation("Employee code retrieved for INN {Inn}", inn);
                return response?.Trim() ?? "DEFAULT";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get employee code for INN {Inn}", inn);
                return "DEFAULT";
            }

        }

    }
}
