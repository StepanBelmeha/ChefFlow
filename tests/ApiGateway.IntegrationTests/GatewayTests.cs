using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ApiGateway.Dtos;
using ApiGateway.IntegrationTests.Fakes;
using ApiGateway.IntegrationTests.Helpers;

namespace ApiGateway.IntegrationTests
{
    public class GatewayTests
    {
        private const string RecipeJson =
            "{\"id\":1,\"userId\":1,\"authorName\":\"Admin\",\"title\":\"Борщ\",\"description\":\"d\",\"instructions\":\"i\",\"ingredients\":[]}";
        private const string TasksJson =
            "[{\"id\":1,\"title\":\"Мариновка\",\"description\":\"\",\"priority\":\"Високий\"}]";

        private static HttpClient Client(GatewayFactory f, int userId = 1)
        {
            var c = f.CreateClient();
            c.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", JwtHelper.GenerateToken(userId));
            return c;
        }

        // 1. Усі сервіси доступні
        [Fact]
        public async Task Screen_AllServicesUp_ReturnsFullResponse()
        {
            var f = new GatewayFactory(
                new StubHandler(_ => StubHandler.Json(RecipeJson)),
                new StubHandler(_ => StubHandler.Json(TasksJson)));

            var res = await Client(f).GetAsync("/api/v1/screens/recipe/1");

            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            var dto = await res.Content.ReadFromJsonAsync<RecipeScreenDto>();
            Assert.Equal("Борщ", dto!.Recipe!.Title);
            Assert.Single(dto.MyTasks!);
            Assert.Empty(dto.UnavailableParts);
        }

        // 2. TaskService недоступний: часткова відповідь
        [Fact]
        public async Task Screen_TaskServiceDown_ReturnsPartialWithMarker()
        {
            var f = new GatewayFactory(
                new StubHandler(_ => StubHandler.Json(RecipeJson)),
                new DownHandler());

            var res = await Client(f).GetAsync("/api/v1/screens/recipe/1");

            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            var dto = await res.Content.ReadFromJsonAsync<RecipeScreenDto>();
            Assert.NotNull(dto!.Recipe);
            Assert.Null(dto.MyTasks);
            Assert.Contains("myTasks", dto.UnavailableParts);
        }

        // 3. Без токена
        [Fact]
        public async Task Screen_WithoutToken_Returns401()
        {
            var stub = new StubHandler(_ => StubHandler.Json(RecipeJson));
            var f = new GatewayFactory(stub, new StubHandler(_ => StubHandler.Json(TasksJson)));

            var res = await f.CreateClient().GetAsync("/api/v1/screens/recipe/1");

            Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
            Assert.Equal(0, stub.Calls);   // у сервіси нічого не пішло
        }

        // 4. Перевищення ліміту на вхід
        [Fact]
        public async Task Login_ExceedingLimit_Returns429WithRetryAfter()
        {
            var f = new GatewayFactory(
                new StubHandler(_ => StubHandler.Json(RecipeJson)),
                new StubHandler(_ => StubHandler.Json(TasksJson)));
            var c = f.CreateClient();

            HttpResponseMessage? last = null;
            for (int i = 0; i < 7; i++)
                last = await c.PostAsJsonAsync("/api/v1/auth/login", new { email = "x@x.com", password = "p" });

            Assert.Equal(HttpStatusCode.TooManyRequests, last!.StatusCode);
            Assert.True(last.Headers.Contains("Retry-After"));
        }

        // 5. Кеш: повторний запит не б'є в RecipeService
        [Fact]
        public async Task Screen_SecondRequest_UsesCache()
        {
            var recipeStub = new StubHandler(_ => StubHandler.Json(RecipeJson));
            var f = new GatewayFactory(recipeStub, new StubHandler(_ => StubHandler.Json(TasksJson)));
            var c = Client(f);

            await c.GetAsync("/api/v1/screens/recipe/1");
            await c.GetAsync("/api/v1/screens/recipe/1");

            Assert.Equal(1, recipeStub.Calls);
        }

        // 6. Персональні дані не кешуються: два користувачі
        [Fact]
        public async Task Screen_TwoUsers_TasksNotCached()
        {
            var tasksStub = new StubHandler(_ => StubHandler.Json(TasksJson));
            var f = new GatewayFactory(new StubHandler(_ => StubHandler.Json(RecipeJson)), tasksStub);

            await Client(f, 1).GetAsync("/api/v1/screens/recipe/1");
            await Client(f, 2).GetAsync("/api/v1/screens/recipe/1");

            Assert.Equal(2, tasksStub.Calls);   // завдання завжди йдуть у сервіс
        }
    }
}