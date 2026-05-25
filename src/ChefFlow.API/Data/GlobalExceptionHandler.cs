using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using static System.Net.Mime.MediaTypeNames;
namespace ChefFlow.API.Data
{
    internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        async ValueTask<bool> IExceptionHandler.TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
                logger.LogError(exception, "Unhandled Exception");
                httpContext.Response.StatusCode = exception switch
                {
                    ApplicationException => StatusCodes.Status400BadRequest,
                    _=> StatusCodes.Status500InternalServerError
                };

            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Type = exception.GetType().Name,
                    Title = "An error occurred.",
                    Detail = exception.Message
                }
            });



    }
}

}

   