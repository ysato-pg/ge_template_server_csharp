namespace GeTemplate.Server.InGame;

using GeTemplate.Server.Common;

/// <summary>
/// /api/ingame/* のエンドポイント。すべてセッショントークンでの認証が必要。
/// </summary>
public static class InGameEndpoints
{
    public static void MapInGameEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/ingame").AddEndpointFilter<SessionAuth>();

        group.MapPost("/start", (StartInGameRequest request, HttpContext context, InGameService service, CancellationToken cancellationToken) =>
            service.StartAsync(SessionAuth.GetSessionUser(context), request, cancellationToken));

        group.MapPost("/end", (EndInGameRequest request, HttpContext context, InGameService service, CancellationToken cancellationToken) =>
            service.EndAsync(SessionAuth.GetSessionUser(context), request, cancellationToken));
    }
}
