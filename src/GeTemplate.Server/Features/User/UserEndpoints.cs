namespace GeTemplate.Server.User;

using GeTemplate.Server.Common;

/// <summary>
/// /api/user/* のエンドポイント。すべてセッショントークンでの認証が必要。
/// </summary>
public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/user").AddEndpointFilter<SessionAuth>();

        group.MapPost("/update-and-fetch", (HttpContext context, UserService service, CancellationToken cancellationToken) =>
            service.UpdateAndFetchAsync(SessionAuth.GetSessionUser(context), cancellationToken));

        group.MapPost("/fetch", (HttpContext context, UserService service) =>
            service.Fetch(SessionAuth.GetSessionUser(context)));
    }
}
