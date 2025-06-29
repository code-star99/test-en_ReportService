using System.Collections.Generic;
using System.Threading.Tasks;
using ReportService.Domain.Entities;

namespace ReportService.Domain.Repositories
{
    public interface IEmployeeRepository
    {
        Task<List<string>> GetActiveDepartmentsAsync();
        Task<List<Employee>> GetEmployeesByDepartmentAsync(string department);

    }
}
