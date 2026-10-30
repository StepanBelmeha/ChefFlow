using System.Security.Cryptography;
using System.Text;
using AuthService.Data;
using ChefFlow.Contracts.Users;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Services;

public sealed class UserDirectoryGrpcService(
    AuthDbContext dbContext,
    IConfiguration configuration) : UserDirectory.UserDirectoryBase
{
    public override async Task<GetUserSummaryResponse> GetUserSummary(
        GetUserSummaryRequest request,
        ServerCallContext context)
    {
        var expectedApiKey = configuration["Services:InternalApiKey"];
        var suppliedApiKey = context.RequestHeaders.GetValue("x-internal-api-key");

        if (string.IsNullOrEmpty(expectedApiKey)
            || string.IsNullOrEmpty(suppliedApiKey)
            || !CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expectedApiKey),
                Encoding.UTF8.GetBytes(suppliedApiKey)))
        {
            throw new RpcException(new Status(
                StatusCode.Unauthenticated,
                "A valid service credential is required."));
        }

        var user = await dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == request.UserId)
            .Select(user => new { user.Id, user.Name })
            .FirstOrDefaultAsync(context.CancellationToken);

        if (user is null)
        {
            throw new RpcException(new Status(
                StatusCode.NotFound,
                "User was not found."));
        }

        return new GetUserSummaryResponse
        {
            UserId = user.Id,
            Name = user.Name
        };
    }
}
