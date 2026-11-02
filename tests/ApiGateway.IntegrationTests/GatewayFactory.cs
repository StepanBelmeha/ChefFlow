using ApiGateway.Clients;
using ApiGateway.IntegrationTests.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace ApiGateway.IntegrationTests
{
    public class GatewayFactory(HttpMessageHandler recipe, HttpMessageHandler tasks)
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(s =>
            {
                s.AddHttpClient<RecipeClient>(c => c.BaseAddress = new Uri("http://stub-recipe"))
                    .ConfigurePrimaryHttpMessageHandler(() => recipe);
                s.AddHttpClient<TaskClient>(c => c.BaseAddress = new Uri("http://stub-task"))
                    .ConfigurePrimaryHttpMessageHandler(() => tasks);
            });
        }
    }
}