using ApiGateway.Clients;
using ApiGateway.Dtos;

namespace ApiGateway.Aggregators
{
    public class RecipeScreenAggregator(RecipeClient recipes, TaskClient tasks,
        ILogger<RecipeScreenAggregator> logger)
    {
        public async Task<RecipeScreenDto?> BuildAsync(int recipeId, string bearerToken, CancellationToken ct)
        {
            var recipeTask = recipes.GetByIdAsync(recipeId, ct);
            var tasksTask = tasks.GetMyTasksAsync(bearerToken, ct);

            try { await Task.WhenAll(recipeTask, tasksTask); } catch { /* стани розбираємо нижче */ }

            // Головна частина екрана: без неї повна відмова
            if (!recipeTask.IsCompletedSuccessfully)
                throw recipeTask.Exception?.GetBaseException()
                      ?? new TaskCanceledException("RecipeService не відповів");

            if (recipeTask.Result == null) return null;

            var dto = new RecipeScreenDto { Recipe = recipeTask.Result };

            if (tasksTask.IsCompletedSuccessfully)
            {
                dto.MyTasks = tasksTask.Result;
            }
            else
            {
                var reason = tasksTask.Exception?.GetBaseException().Message ?? "таймаут";
                logger.LogWarning("TaskService недоступний: {Reason}", reason);
                dto.UnavailableParts.Add("myTasks");   // null ≠ "завдань немає"
            }

            return dto;
        }
    }
}
