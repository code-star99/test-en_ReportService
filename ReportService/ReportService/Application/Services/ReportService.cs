using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ReportService.Domain.Repositories;
using ReportService.Domain.Services;

namespace ReportService.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<ReportService> _logger;

        public ReportService(
            IEmployeeRepository employeeRepository,
            IEmployeeService employeeService,
            ILogger<ReportService> logger)
        {
            _employeeRepository = employeeRepository;
            _employeeService = employeeService;
            _logger = logger;
        }

        public async Task<FileResult> GenerateReportAsync(int year, int month)
        {
            _logger.LogInformation("Generating report for {Year}/{Month}", year, month);

            try
            {
                var reportContent = await GenerateReportContentAsync(year, month);
                var fileName = $"report_{year}_{month}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                var fileBytes = Encoding.UTF8.GetBytes(reportContent);

                _logger.LogInformation("Report generated successfully for {Year}/{Month}", year, month);

                return new FileContentResult(fileBytes, "text/plain")
                {
                    FileDownloadName = fileName
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to generate report for {Year}/{Month}", year, month);
                throw;
            }
        }

        private async Task<string> GenerateReportContentAsync(int year, int month)
        {
            var monthName = new DateTime(year, month, 1).ToString("MMMM");
            var reportBuilder = new StringBuilder();

            reportBuilder.AppendLine($"{monthName} {year}");
            reportBuilder.AppendLine();

            var departments = await _employeeRepository.GetActiveDepartmentsAsync();
            var totalCompanySalary = 0m;

            foreach (var department in departments)
            {
                var employees = await _employeeRepository.GetEmployeesByDepartmentAsync(department);
                var departmentSalary = 0m;

                reportBuilder.AppendLine($"### {department}");
                reportBuilder.AppendLine();

                foreach (var employee in employees)
                {
                    var employeeCode = await _employeeService.GetEmployeeCodeAsync(employee.Inn);
                    var salary = await _employeeService.CalculateSalaryAsync(employee);

                    employee.UpdateEmployeeCode(employeeCode);
                    employee.UpdateSalary(salary);

                    reportBuilder.AppendLine($"| {employee.Name} | {salary:C} |");
                    departmentSalary += salary;
                }

                reportBuilder.AppendLine();
                reportBuilder.AppendLine($"**Department Total: {departmentSalary:C}**");
                reportBuilder.AppendLine();

                totalCompanySalary += departmentSalary;
            }

            reportBuilder.AppendLine($"### Company Total: **{totalCompanySalary:C}**");

            return reportBuilder.ToString();
        }
    }
}