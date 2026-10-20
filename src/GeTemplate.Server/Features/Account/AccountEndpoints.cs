namespace GeTemplate.Server.Account;

/// <summary>
/// /api/account/* のエンドポイント。
/// </summary>
public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/account");

        group.MapPost("/create", (AccountService service, CancellationToken cancellationToken) =>
            service.CreateAsync(cancellationToken));

        group.MapPost("/login", (LoginRequest? request, AccountService service, CancellationToken cancellationToken) =>
            service.LoginAsync(request ?? new LoginRequest(null, null), cancellationToken));
    }
}
