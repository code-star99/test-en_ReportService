using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using ReportService.Domain.Entities;
using ReportService.Domain.Repositories;
using System;
using System.Collections.Generic;
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
    }
}
