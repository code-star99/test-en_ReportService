
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace ReportService.Tests
{
    public class ReportApiTests
    {
        private readonly TestServer _server;
        private readonly HttpClient _client;

        public ReportApiTests() 
        {
            _server = new TestServer(new WebHostBuilder()
                .UseEnvironment("Development")
                .UseStartup<ReportService.Startup>());
            _client = _server.CreateClient();
        }

        [Fact]
        public async Task GetReport_ReturnsReportFile()
        {
            var response = await _client.GetAsync("api/report/2017/1");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Company Total", content);
            Assert.Contains("Finance Department", content);
        }
    }
}
