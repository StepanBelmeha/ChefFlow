using System.Net.Http.Headers;
using System.Net.Http.Json;
using ApiGateway.Dtos;

namespace ApiGateway.Clients
{
    public class TaskClient(HttpClient http)
    {
        public async Task<List<TaskInfoDto>> GetMyTasksAsync(string bearerToken, CancellationToken ct)
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, "/api/v1/tasks");
            req.Headers.Authorization = AuthenticationHeaderValue.Parse(bearerToken);
            var response = await http.SendAsync(req, ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<TaskInfoDto>>(ct) ?? new();
        }
    }
}
