using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ReportService.Domain.Entities;
using System;
using System.Collections.Generic;
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
            if (ShouldUseMockData()) return await GetMockSalaryAsync(employee);            

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
            if (ShouldUseMockData()) return await GetMockEmployeeCodeAsync(inn);            

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

        private bool ShouldUseMockData()
        {
            var useMockData = _configuration["UseMockData"];
            return !string.IsNullOrEmpty(useMockData) && useMockData.ToLower() == "true";
        }

        private async Task<string> GetMockEmployeeCodeAsync(string inn)
        {
            await Task.Delay(10); 
            _logger.LogInformation("Using mock data for employee code for INN {Inn}", inn);

            var mockEmployeeCodes = new Dictionary<string, string>
            {
                ["123456789"] = "EMPCODE#001",
                ["234567890"] = "EMPCODE#002", 
                ["345678901"] = "EMPCODE#003",
                ["456789012"] = "EMPCODE#004",
                ["567890123"] = "EMPCODE#005",
                ["678901234"] = "EMPCODE#006",
                ["789012345"] = "EMPCODE#007",
                ["890123456"] = "EMPCODE#008",
                ["901234567"] = "EMPCODE#009",
                ["012345678"] = "EMPCODE#010",
                ["123456780"] = "EMPCODE#011"
            };

            return mockEmployeeCodes.ContainsKey(inn) ? mockEmployeeCodes[inn] : "DEFAULT";
        }

        private async Task<decimal> GetMockSalaryAsync(Employee employee)
        {
            await Task.Delay(10);
            _logger.LogInformation("Using mock data for salary for {EmployeeName}", employee?.Name);

            var mockSalaries = new Dictionary<string, decimal>
            {
                ["Andrew Barnes"] = 2200m,
                ["Gregory Evans"] = 2000m,
                ["Jacob Smith"] = 2500m,
                ["Alex Ryan"] = 2700m,
                ["William Johnson"] = 1800m,
                ["Damian Carter"] = 2000m,
                ["Michael Anderson"] = 1500m,
                ["Philip Rogers"] = 2700m,
                ["Dmitry Collins"] = 3500m,
                ["Andrew Miller"] = 3200m,
                ["Arvid Nelson"] = 3500m
            };

            return mockSalaries.ContainsKey(employee?.Name) ? mockSalaries[employee.Name] : 0m;
        }
    }
}
