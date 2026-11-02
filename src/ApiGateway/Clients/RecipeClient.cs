using System.Net.Http.Json;
using ApiGateway.Dtos;
using Microsoft.Extensions.Caching.Memory;

namespace ApiGateway.Clients
{
    public class RecipeClient(HttpClient http, IMemoryCache cache, ILogger<RecipeClient> logger)
    {
        public static string Key(int id) => $"recipe:{id}";

        public async Task<RecipeInfoDto?> GetByIdAsync(int id, CancellationToken ct)
        {
            return await cache.GetOrCreateAsync(Key(id), async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);
                var response = await http.GetAsync($"/api/v1/recipe/{id}", ct);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
                response.EnsureSuccessStatusCode();
                logger.LogInformation("Рецепт {Id} завантажено з RecipeService (кеш промах)", id);
                return await response.Content.ReadFromJsonAsync<RecipeInfoDto>(ct);
            });
        }
    }
}
