using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TaskService.Dtos;
using TaskService.IntegrationTests.Helpers;

namespace TaskService.IntegrationTests
{
    public class TasksControllerTests : IClassFixture<ApiFactory>
    {
        private readonly HttpClient _client;

        public TasksControllerTests(ApiFactory factory)
        {
            _client = factory.CreateClient();
        }

        private void Authorize(int userId = 1)
        {
            var token = JwtHelper.GenerateToken(userId);
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        [Fact]
        public async Task Create_WithValidData_Returns201()
        {
            Authorize();
            var dto = new CreateTaskDto
            {
                Title = "Тестове завдання",
                Description = "Опис",
                Priority = "Високий",
                Deadline = DateTime.UtcNow.AddDays(3)
            };

            var response = await _client.PostAsJsonAsync("/api/v1/tasks", dto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<TaskDto>();
            Assert.NotNull(result);
            Assert.Equal("Тестове завдання", result.Title);
            Assert.Equal(1, result.UserId);
        }

        [Fact]
        public async Task Create_WithoutToken_Returns401()
        {
            _client.DefaultRequestHeaders.Authorization = null;
            var dto = new CreateTaskDto
            {
                Title = "Без токена",
                Priority = "Низький"
            };

            var response = await _client.PostAsJsonAsync("/api/v1/tasks", dto);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Create_WithoutTitle_Returns400()
        {
            Authorize();
            var dto = new CreateTaskDto
            {
                Title = "",
                Priority = "Високий"
            };

            var response = await _client.PostAsJsonAsync("/api/v1/tasks", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetById_OwnTask_Returns200()
        {
            Authorize();
            var dto = new CreateTaskDto
            {
                Title = "Моє завдання",
                Priority = "Середній"
            };
            var createResponse = await _client.PostAsJsonAsync("/api/v1/tasks", dto);
            var created = await createResponse.Content.ReadFromJsonAsync<TaskDto>();

            var response = await _client.GetAsync($"/api/v1/tasks/{created!.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<TaskDto>();
            Assert.Equal("Моє завдання", result!.Title);
        }

        [Fact]
        public async Task GetById_NotFound_Returns404()
        {
            Authorize();

            var response = await _client.GetAsync("/api/v1/tasks/99999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }


        [Fact]
        public async Task GetAll_Returns200WithList()
        {
            Authorize(userId: 2);
            var dto = new CreateTaskDto { Title = "Завдання юзера 2", Priority = "Низький" };
            await _client.PostAsJsonAsync("/api/v1/tasks", dto);

            var response = await _client.GetAsync("/api/v1/tasks");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var tasks = await response.Content.ReadFromJsonAsync<List<TaskDto>>();
            Assert.NotNull(tasks);
            Assert.True(tasks.Count > 0);
        }

        [Fact]
        public async Task Update_Returns200()
        {
            Authorize();
            var create = new CreateTaskDto { Title = "Стара назва", Priority = "Низький" };
            var createResponse = await _client.PostAsJsonAsync("/api/v1/tasks", create);
            var created = await createResponse.Content.ReadFromJsonAsync<TaskDto>();

            var update = new UpdateTaskDto
            {
                Title = "Нова назва",
                Description = "Новий опис",
                Priority = "Високий"
            };
            var response = await _client.PutAsJsonAsync($"/api/v1/tasks/{created!.Id}", update);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<TaskDto>();
            Assert.Equal("Нова назва", result!.Title);
        }

        [Fact]
        public async Task Delete_Returns204()
        {
            Authorize();
            var create = new CreateTaskDto { Title = "Видалити мене", Priority = "Низький" };
            var createResponse = await _client.PostAsJsonAsync("/api/v1/tasks", create);
            var created = await createResponse.Content.ReadFromJsonAsync<TaskDto>();

            var response = await _client.DeleteAsync($"/api/v1/tasks/{created!.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Перевіряємо що справді видалено
            var getResponse = await _client.GetAsync($"/api/v1/tasks/{created.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task Search_Returns200WithResults()
        {
            Authorize(userId: 3);
            await _client.PostAsJsonAsync("/api/v1/tasks", new CreateTaskDto
            {
                Title = "Борщ з буряком",
                Priority = "Середній"
            });

            var response = await _client.GetAsync("/api/v1/tasks/search?q=борщ");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var tasks = await response.Content.ReadFromJsonAsync<List<TaskDto>>();
            Assert.NotNull(tasks);
            Assert.True(tasks.Count > 0);
            Assert.Contains(tasks, t => t.Title.Contains("Борщ"));
        }
    }
}
