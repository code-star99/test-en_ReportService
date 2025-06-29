using System.Threading.Tasks;
using ReportService.Domain.Entities;

namespace ReportService.Domain.Services
{
    public interface IEmployeeService
    {
       Task<decimal> CalculateSalaryAsync(Employee employee);
       Task<string> GetEmployeeCodeAsync(string inn);

    }
}
