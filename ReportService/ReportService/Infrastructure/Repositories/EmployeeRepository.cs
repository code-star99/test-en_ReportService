using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using ReportService.Domain.Entities;
using ReportService.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportService.Infrastructure.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmployeeRepository> _logger;

        public EmployeeRepository(IConfiguration configuration, ILogger<EmployeeRepository> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<List<string>> GetActiveDepartmentsAsync()
        {
            if (ShouldUseMockData()) return await GetMockActiveDepartmentsAsync();            

            var departments = new List<string>();
            var connectionString = _configuration["Database:ConnectionString"];

            try
            {
                var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();

                var command = new NpgsqlCommand("SELECT d.name FROM deps d WHERE d.active = true", connection);
                var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    departments.Add(reader.GetString(0));
                }

                _logger.LogInformation("Retrieved {Count} active departments", departments.Count);
                return departments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve active departments");
                throw;
            }
        }

        public async Task<List<Employee>> GetEmployeesByDepartmentAsync(string department)
        {
            if (ShouldUseMockData())
            {
                return await GetMockEmployeesByDepartmentAsync(department);
            }

            var employees = new List<Employee>();
            var connectionString = _configuration["Database:ConnectionString"];

            try
            {
                var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();

                var command = new NpgsqlCommand(
                    "SELECT e.name, e.inn, d.name FROM emps e LEFT JOIN deps d ON e.departmentid = d.id WHERE d.name = @department",
                    connection);
                command.Parameters.AddWithValue("@department", department);

                var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var employee = new Employee(
                        reader.GetString(0),
                        reader.GetString(1),
                        reader.GetString(2));
                    employees.Add(employee);
                }

                _logger.LogInformation("Retrieved {Count} employees for department {Department}", employees.Count, department);
                return employees;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve employees for department {Department}", department);
                throw;
            }
        }

        private bool ShouldUseMockData()
        {
            var useMockData = _configuration["UseMockData"];
            return !string.IsNullOrEmpty(useMockData) && useMockData.ToLower() == "true";
        }

        private async Task<List<string>> GetMockActiveDepartmentsAsync()
        {
            await Task.Delay(10); // Simulate async operation
            _logger.LogInformation("Using mock data for active departments");
            
            return new List<string>
            {
                "Finance Department",
                "Accounting",
                "IT"
            };
        }

        private async Task<List<Employee>> GetMockEmployeesByDepartmentAsync(string department)
        {
            await Task.Delay(10); // Simulate async operation
            _logger.LogInformation("Using mock data for employees in department {Department}", department);

            var mockEmployees = new Dictionary<string, List<Employee>>
            {
                ["Finance Department"] = new List<Employee>
                {
                    new Employee("Andrew Barnes", "123456789", "Finance Department"),
                    new Employee("Gregory Evans", "234567890", "Finance Department"),
                    new Employee("Jacob Smith", "345678901", "Finance Department"),
                    new Employee("Alex Ryan", "456789012", "Finance Department")
                },
                ["Accounting"] = new List<Employee>
                {
                    new Employee("William Johnson", "567890123", "Accounting"),
                    new Employee("Damian Carter", "678901234", "Accounting"),
                    new Employee("Michael Anderson", "789012345", "Accounting")
                },
                ["IT"] = new List<Employee>
                {
                    new Employee("Philip Rogers", "890123456", "IT"),
                    new Employee("Dmitry Collins", "901234567", "IT"),
                    new Employee("Andrew Miller", "012345678", "IT"),
                    new Employee("Arvid Nelson", "123456780", "IT")
                }
            };

            return mockEmployees.ContainsKey(department) ? mockEmployees[department] : new List<Employee>();
        }
    }
}
