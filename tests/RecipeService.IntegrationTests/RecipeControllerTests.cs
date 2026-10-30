using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using RecipeService.DTO;
using RecipeService.IntegrationTests.Fakes;
using RecipeService.IntegrationTests.Helpers;

namespace RecipeService.IntegrationTests
{
    public class RecipeControllerTests : IDisposable
    {
        private readonly ApiFactory _factory;
        private readonly HttpClient _client;
        private static int _userIdCounter = 100;

        public RecipeControllerTests()
        {
            _factory = new ApiFactory();
            _client = _factory.CreateClient();
        }

        public void Dispose()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        private int NextUserId() => Interlocked.Increment(ref _userIdCounter);

        private void Authorize(int userId)
        {
            var token = JwtHelper.GenerateToken(userId);
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        [Fact]
        public async Task GetAll_WithoutToken_Returns200()
        {
            var response = await _client.GetAsync("/api/v1/recipe");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithToken_Returns201()
        {
            var userId = NextUserId();
            Authorize(userId);

            var dto = new CreateRecipeDto
            {
                Title = "Тестовий рецепт",
                Description = "Опис",
                Instructions = "Кроки приготування",
                IsPublished = true,
                Ingredients = new List<CreateIngredientDto>
                {
                    new() { ProductName = "борошно", Quantity = 200, Unit = "г" }
                }
            };

            var response = await _client.PostAsJsonAsync("/api/v1/recipe", dto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<RecipeDto>();
            Assert.NotNull(result);
            Assert.Equal("Тестовий рецепт", result.Title);
            Assert.Equal(userId, result.UserId);
        }

        [Fact]
        public async Task Create_WithoutToken_Returns401()
        {
            _client.DefaultRequestHeaders.Authorization = null;

            var dto = new CreateRecipeDto
            {
                Title = "Без токена",
                Instructions = "...",
                Ingredients = new()
            };

            var response = await _client.PostAsJsonAsync("/api/v1/recipe", dto);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetById_ReturnsAuthorName_FromUserDirectory()
        {
            Authorize(1); // userId=1 є в FakeAuthHandler як "Admin"

            var dto = new CreateRecipeDto
            {
                Title = "Рецепт з автором",
                Instructions = "...",
                Ingredients = new()
            };
            var createResponse = await _client.PostAsJsonAsync("/api/v1/recipe", dto);
            var created = await createResponse.Content.ReadFromJsonAsync<RecipeDto>();

            var response = await _client.GetAsync($"/api/v1/recipe/{created!.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<RecipeDetailsDto>();
            Assert.NotNull(result);
            Assert.Equal("Admin", result.AuthorName);
        }

        [Fact]
        public async Task GetById_WhenAuthServiceUnavailable_ReturnsUnknownAuthor()
        {
            var factory = new ApiFactory(new UnavailableUserDirectoryCallInvoker());
            var client = factory.CreateClient();

            var token = JwtHelper.GenerateToken(1);
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var dto = new CreateRecipeDto
            {
                Title = "Рецепт без автора",
                Instructions = "...",
                Ingredients = new()
            };
            var createResponse = await client.PostAsJsonAsync("/api/v1/recipe", dto);
            var created = await createResponse.Content.ReadFromJsonAsync<RecipeDto>();

            var response = await client.GetAsync($"/api/v1/recipe/{created!.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<RecipeDetailsDto>();
            Assert.Equal("Невідомий автор", result!.AuthorName);

            client.Dispose();
            factory.Dispose();
        }

        [Fact]
        public async Task GetById_NotFound_Returns404()
        {
            Authorize(NextUserId());
            var response = await _client.GetAsync("/api/v1/recipe/99999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Publish_Returns204()
        {
            var userId = NextUserId();
            Authorize(userId);

            var dto = new CreateRecipeDto
            {
                Title = "Чернетка",
                Instructions = "...",
                IsPublished = false,
                Ingredients = new()
            };
            var createResponse = await _client.PostAsJsonAsync("/api/v1/recipe", dto);
            var created = await createResponse.Content.ReadFromJsonAsync<RecipeDto>();

            var response = await _client.PatchAsync(
                $"/api/v1/recipe/{created!.Id}/publish", null);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task GetAll_ReturnsAuthorName()
        {
            Authorize(1);

            await _client.PostAsJsonAsync("/api/v1/recipe", new CreateRecipeDto
            {
                Title = "Рецепт для списку",
                Instructions = "...",
                IsPublished = true,
                Ingredients = new()
            });

            var response = await _client.GetAsync("/api/v1/recipe");
            var result = await response.Content
                .ReadFromJsonAsync<PagedResult<RecipeDto>>();

            Assert.NotNull(result);
            Assert.True(result.Items.Count > 0);
            Assert.All(result.Items, r => Assert.False(string.IsNullOrEmpty(r.AuthorName)));
        }

        [Fact]
        public async Task GetAll_WithSearchAndPagination_ReturnsFilteredResults()
        {
            var userId = NextUserId();
            Authorize(userId);

            // Створюємо кілька рецептів
            await _client.PostAsJsonAsync("/api/v1/recipe", new CreateRecipeDto
            {
                Title = "Борщ український",
                Instructions = "...",
                IsPublished = true,
                Ingredients = new()
            });
            await _client.PostAsJsonAsync("/api/v1/recipe", new CreateRecipeDto
            {
                Title = "Вареники з картоплею",
                Instructions = "...",
                IsPublished = true,
                Ingredients = new()
            });
            await _client.PostAsJsonAsync("/api/v1/recipe", new CreateRecipeDto
            {
                Title = "Борщ зелений",
                Instructions = "...",
                IsPublished = true,
                Ingredients = new()
            });

            var response = await _client.GetAsync(
                "/api/v1/recipe?search=Борщ&page=1&pageSize=10");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content
                .ReadFromJsonAsync<PagedResult<RecipeDto>>();

            Assert.NotNull(result);
            Assert.Equal(1, result.Page);
            Assert.True(result.TotalCount >= 2);
            Assert.All(result.Items, r => Assert.Contains("Борщ", r.Title));

            }
    }
}
