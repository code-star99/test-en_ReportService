using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReportService.Application.Services;
using ReportService.Domain.Repositories;
using ReportService.Domain.Services;
using ReportService.Infrastructure.Repositories;
using System.Net.Http;

namespace ReportService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            // Register HttpClient for external service calls (.NET Core 2.0 approach)
            services.AddSingleton<HttpClient>();

            // Register repositories
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();

            // Register domain services
            services.AddScoped<IEmployeeService, EmployeeService>();

            // Register application services
            services.AddScoped<IReportService, ReportService.Application.Services.ReportService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
