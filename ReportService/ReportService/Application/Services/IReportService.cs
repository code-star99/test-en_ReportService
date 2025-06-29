using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ReportService.Application.Services
{
    public interface IReportService
    {
       Task<FileResult> GenerateReportAsync(int year, int month);
    }
}
