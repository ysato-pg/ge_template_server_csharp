namespace GeTemplate.Server.Common;

using GeTemplate.Server.Data;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Authorization: Bearer のセッショントークンでユーザーを探すエンドポイントフィルタ。
/// 見つからなければ InvalidSession。見つかったユーザーは GetSessionUser() で取り出す。
/// </summary>
public sealed class SessionAuth(GameDbContext db) : IEndpointFilter
{
    private const string BearerPrefix = "Bearer ";
    private static readonly object UserKey = new();

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var token = ReadToken(httpContext.Request.Headers.Authorization.ToString());
        if (token is null)
        {
            throw ApiException.InvalidSession();
        }

        var user = await db.Users.SingleOrDefaultAsync(u => u.SessionToken == token, httpContext.RequestAborted)
            ?? throw ApiException.InvalidSession();

        httpContext.Items[UserKey] = user;
        return await next(context);
    }

    /// <summary>SessionAuth で認証したユーザー。SessionAuth を付けたエンドポイントでだけ使う。</summary>
    public static User GetSessionUser(HttpContext context) =>
        context.Items[UserKey] as User
        ?? throw new InvalidOperationException("SessionAuth が付いていないエンドポイントです。");

    private static string? ReadToken(string header)
    {
        if (!header.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }
        var token = header[BearerPrefix.Length..].Trim();
        return token.Length == 0 ? null : token;
    }
}
