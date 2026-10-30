using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskService.Data;

namespace TaskService.IntegrationTests
{
    public class ApiFactory : WebApplicationFactory<Program>
    {
        private const string DbName = "TestDb_Shared";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                var descriptors = services
                    .Where(d =>
                        d.ServiceType == typeof(DbContextOptions<TaskDbContext>) ||
                        d.ServiceType == typeof(TaskDbContext) ||
                        d.ServiceType.FullName?.Contains("EntityFramework") == true ||
                        d.ServiceType.FullName?.Contains("MySql") == true)
                    .ToList();

                foreach (var d in descriptors)
                    services.Remove(d);

                services.AddDbContext<TaskDbContext>(options =>
                    options.UseInMemoryDatabase(DbName));
            });
        }
    }
}