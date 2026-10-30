using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ChefFlow.Contracts.Users;
using RecipeService.Data;
using RecipeService.IntegrationTests.Fakes;

namespace RecipeService.IntegrationTests
{
    public class ApiFactory : WebApplicationFactory<Program>
    {
        private readonly Grpc.Core.CallInvoker _userDirectoryCallInvoker;
        private readonly string _databaseName = "RecipeTestDb_" + Guid.NewGuid();

        public ApiFactory(Grpc.Core.CallInvoker? userDirectoryCallInvoker = null)
        {
            _userDirectoryCallInvoker = userDirectoryCallInvoker ?? new FakeUserDirectoryCallInvoker();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("Services:InternalApiKey", "integration-test-key-at-least-32-bytes");
            builder.UseSetting("Services:AuthGrpc", "https://localhost:5005");
            builder.ConfigureServices(services =>
            {
                // Прибираємо реальний DbContext
                var descriptors = services
                    .Where(d =>
                        d.ServiceType == typeof(DbContextOptions<RecipeDbContext>) ||
                        d.ServiceType == typeof(RecipeDbContext) ||
                        d.ServiceType.FullName?.Contains("EntityFramework") == true ||
                        d.ServiceType.FullName?.Contains("MySql") == true)
                    .ToList();

                foreach (var d in descriptors)
                    services.Remove(d);

                services.AddDbContext<RecipeDbContext>(options =>
                    options.UseInMemoryDatabase(_databaseName));

                services.RemoveAll<UserDirectory.UserDirectoryClient>();
                services.AddSingleton(new UserDirectory.UserDirectoryClient(_userDirectoryCallInvoker));
            });
        }
    }
}