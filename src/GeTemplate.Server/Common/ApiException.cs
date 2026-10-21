namespace GeTemplate.Server.Common;

/// <summary>
/// HTTP ステータスと errorCode を持つ例外。ApiErrorHandling がエラー応答に変換する。
/// </summary>
public sealed class ApiException(int statusCode, string errorCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string ErrorCode { get; } = errorCode;

    public static ApiException InvalidRequest(string message) =>
        new(StatusCodes.Status400BadRequest, ApiErrorCodes.InvalidRequest, message);

    public static ApiException InvalidCredentials() =>
        new(StatusCodes.Status401Unauthorized, ApiErrorCodes.InvalidCredentials, "ユーザー ID または認証トークンが正しくありません。");

    public static ApiException InvalidSession() =>
        new(StatusCodes.Status401Unauthorized, ApiErrorCodes.InvalidSession, "セッショントークンが無いか、正しくありません。");

    public static ApiException InvalidPlay() =>
        new(StatusCodes.Status409Conflict, ApiErrorCodes.InvalidPlay, "進行中のインゲームと一致しません（開始していない・終了済み・置き換え済み）。");
}

/// <summary>
/// 例外を共通のエラー応答 { errorCode, message } に変換するミドルウェア。
/// </summary>
public static class ApiErrorHandling
{
    public static IApplicationBuilder UseApiErrorHandling(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            try
            {
                await next(context);
            }
            catch (ApiException e)
            {
                await WriteErrorAsync(context, e.StatusCode, new ApiError(e.ErrorCode, e.Message));
            }
            catch (BadHttpRequestException e)
            {
                // 本文が JSON として読めないなど、リクエストの形式が正しくない。
                await WriteErrorAsync(context, StatusCodes.Status400BadRequest, new ApiError(ApiErrorCodes.InvalidRequest, e.Message));
            }
            catch (Exception e)
            {
                var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(ApiErrorHandling));
                logger.LogError(e, "想定外のエラーが発生しました。{Path}", context.Request.Path);
                await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, new ApiError(ApiErrorCodes.InternalError, "サーバ内部でエラーが発生しました。"));
            }
        });
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, ApiError error)
    {
        if (context.Response.HasStarted)
        {
            return;
        }
        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(error);
    }
}
