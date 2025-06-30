using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ReportService.Domain.Repositories;
using ReportService.Domain.Services;
using ReportService.Domain.Entities;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

namespace ReportService.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<ReportService> _logger;
        private static readonly int ConcurrencyLimit = 5;
        private static readonly SemaphoreSlim CodeSemaphore = new SemaphoreSlim(ConcurrencyLimit);
        private static readonly SemaphoreSlim SalarySemaphore = new SemaphoreSlim(ConcurrencyLimit);

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
            try
            {
                var reportContent = await GenerateReportContentAsync(year, month);
                var fileName = $"report_{year}_{month}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                var fileBytes = Encoding.UTF8.GetBytes(reportContent);

                return new FileContentResult(fileBytes, "text/plain")
                {
                    FileDownloadName = fileName
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in generating report for {Year}/{Month}", year, month);
                
                var fallbackContent = GenerateFallbackReport(year, month, e);
                var fileName = $"report_{year}_{month}_{DateTime.Now:yyyyMMdd_HHmmss}_ERROR.txt";
                var fileBytes = Encoding.UTF8.GetBytes(fallbackContent);

                return new FileContentResult(fileBytes, "text/plain")
                {
                    FileDownloadName = fileName
                };
            }
        }

        private async Task<string> GenerateReportContentAsync(int year, int month)
        {
            var monthName = new DateTime(year, month, 1).ToString("MMMM");
            var reportBuilder = new StringBuilder();
            var totalCompanySalary = 0m;
            var successfulDepartments = 0;
            var failedDepartments = 0;

            reportBuilder.AppendLine($"{monthName} {year}");
            reportBuilder.AppendLine();

            List<string> departments;
            try
            {
                departments = await _employeeRepository.GetActiveDepartmentsAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to retrieve departments for {Year}/{Month}", year, month);
                reportBuilder.AppendLine("**Error: Could not retrieve departments.**");
                reportBuilder.AppendLine();
                reportBuilder.AppendLine($"### Company Total: **{totalCompanySalary:C}**");
                reportBuilder.AppendLine();
                return reportBuilder.ToString();
            }

            foreach (var department in departments)
            {
                List<Employee> employees;
                try
                {
                    employees = await _employeeRepository.GetEmployeesByDepartmentAsync(department);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to fetch employees for department {Department}", department);
                    reportBuilder.AppendLine($"### {department}");
                    reportBuilder.AppendLine("**Error: Could not retrieve employees for this department.**");
                    reportBuilder.AppendLine();
                    failedDepartments++;
                    continue; // Move to next department - 1, 3
                }

                var departmentSalary = 0m;
                var departmentEmployeeCount = 0;

                reportBuilder.AppendLine($"### {department}");
                reportBuilder.AppendLine();

                try
                {
                    // Parallelize fetching employee codes with concurrency limit - 2
                    var codeTasks = employees.Select(async e =>
                    {
                        await CodeSemaphore.WaitAsync();
                        try
                        {
                            return await _employeeService.GetEmployeeCodeAsync(e.Inn);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to get employee code for {EmployeeName} (INN: {Inn})", e.Name, e.Inn);
                            return "ERROR";
                        }
                        finally
                        {
                            CodeSemaphore.Release();
                        }
                    }).ToArray();
                    var codes = await Task.WhenAll(codeTasks);

                    // Update employee codes before fetching salaries
                    for (int i = 0; i < employees.Count; i++)
                    {
                        employees[i].UpdateEmployeeCode(codes[i]);
                    }

                    // Parallelize fetching salaries with concurrency limit - 2
                    var salaryTasks = employees.Select(async e =>
                    {
                        await SalarySemaphore.WaitAsync();
                        try
                        {
                            return await _employeeService.CalculateSalaryAsync(e);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to get salary for {EmployeeName} (INN: {Inn})", e.Name, e.Inn);
                            return 0m;
                        }
                        finally
                        {
                            SalarySemaphore.Release();
                        }
                    }).ToArray();
                    var salaries = await Task.WhenAll(salaryTasks);

                    for (int i = 0; i < employees.Count; i++)
                    {
                        employees[i].UpdateSalary(salaries[i]);
                        var salaryDisplay = salaries[i] > 0 ? salaries[i].ToString("C") : "N/A";
                        reportBuilder.AppendLine($"| {employees[i].Name} | {salaryDisplay} |");
                        departmentSalary += salaries[i];
                        departmentEmployeeCount++;
                    }

                    reportBuilder.AppendLine();
                    reportBuilder.AppendLine($"**Department Total: {departmentSalary:C}**");
                    reportBuilder.AppendLine();

                    totalCompanySalary += departmentSalary;
                    successfulDepartments++;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to process department {Department}", department);
                    reportBuilder.AppendLine("**Error: Could not process department data.**");
                    reportBuilder.AppendLine();
                    failedDepartments++;
                }
            }

            // Company total section - 4
            reportBuilder.AppendLine($"### Company Total: **{totalCompanySalary:C}**");
            reportBuilder.AppendLine();

            // Add summary information - added by Konrad
            if (successfulDepartments > 0 || failedDepartments > 0)
            {
                reportBuilder.AppendLine("---");
                reportBuilder.AppendLine($"**Report Summary:**");
                reportBuilder.AppendLine($"- Successful departments: {successfulDepartments}");
                reportBuilder.AppendLine($"- Failed departments: {failedDepartments}");
                reportBuilder.AppendLine($"- Total departments processed: {successfulDepartments + failedDepartments}");
                
                if (failedDepartments > 0)
                {
                    reportBuilder.AppendLine($"- *Note: Some departments could not be processed. Company total reflects only successful departments.*");
                }
            }

            return reportBuilder.ToString();
        }

        private string GenerateFallbackReport(int year, int month, Exception error)
        {
            var monthName = new DateTime(year, month, 1).ToString("MMMM");
            var reportBuilder = new StringBuilder();

            reportBuilder.AppendLine($"{monthName} {year}");
            reportBuilder.AppendLine();
            reportBuilder.AppendLine("**SYSTEM ERROR - FALLBACK REPORT**");
            reportBuilder.AppendLine();
            reportBuilder.AppendLine("The report service encountered a critical error and could not generate a complete report.");
            reportBuilder.AppendLine();
            reportBuilder.AppendLine("**Error Details:**");
            reportBuilder.AppendLine($"- Error Type: {error.GetType().Name}");
            reportBuilder.AppendLine($"- Error Message: {error.Message}");
            reportBuilder.AppendLine($"- Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            reportBuilder.AppendLine();
            reportBuilder.AppendLine("**Troubleshooting:**");
            reportBuilder.AppendLine("- Check system logs for detailed error information");
            reportBuilder.AppendLine("- Verify database connectivity");
            reportBuilder.AppendLine("- Ensure external services are available");
            reportBuilder.AppendLine("- Contact system administrator if the issue persists");
            reportBuilder.AppendLine();
            reportBuilder.AppendLine("### Company Total: **€0.00**");
            reportBuilder.AppendLine();
            reportBuilder.AppendLine("*Note: This is a fallback report generated due to system errors.*");

            return reportBuilder.ToString();
        }
    }
}