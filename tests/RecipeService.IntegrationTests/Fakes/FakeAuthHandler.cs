using ChefFlow.Contracts.Users;
using Grpc.Core;

namespace RecipeService.IntegrationTests.Fakes
{
    public class FakeUserDirectoryCallInvoker : CallInvoker
    {
        private readonly Dictionary<int, string> _users;

        public FakeUserDirectoryCallInvoker(Dictionary<int, string>? users = null)
        {
            _users = users ?? new Dictionary<int, string>
            {
                { 1, "Admin" },
                { 2, "Тест Користувач" }
            };
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            string? host,
            CallOptions options,
            TRequest request)
        {
            var userId = ((GetUserSummaryRequest)(object)request!).UserId;
            if (!_users.TryGetValue(userId, out var name))
                throw new RpcException(new Status(StatusCode.NotFound, "User was not found."));

            var response = (TResponse)(object)new GetUserSummaryResponse
            {
                UserId = userId,
                Name = name
            };

            return new AsyncUnaryCall<TResponse>(
                Task.FromResult(response),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });
        }

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            string? host,
            CallOptions options,
            TRequest request) =>
            throw new NotSupportedException();

        public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            string? host,
            CallOptions options) =>
            throw new NotSupportedException();

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            string? host,
            CallOptions options,
            TRequest request) =>
            throw new NotSupportedException();

        public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            string? host,
            CallOptions options) =>
            throw new NotSupportedException();
    }

    public sealed class UnavailableUserDirectoryCallInvoker : FakeUserDirectoryCallInvoker
    {
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            string? host,
            CallOptions options,
            TRequest request)
        {
            var response = Task.FromException<TResponse>(
                new RpcException(new Status(StatusCode.Unavailable, "AuthService unavailable.")));

            return new AsyncUnaryCall<TResponse>(
                response,
                Task.FromResult(new Metadata()),
                () => new Status(StatusCode.Unavailable, "AuthService unavailable."),
                () => new Metadata(),
                () => { });
        }
    }
}