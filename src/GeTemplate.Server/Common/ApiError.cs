namespace GeTemplate.Server.Common;

/// <summary>
/// エラー応答の本文。common/architecture.md の「エラー形式」に従う。
/// </summary>
public sealed record ApiError(string ErrorCode, string Message);

/// <summary>
/// エラー応答の errorCode。
/// </summary>
public static class ApiErrorCodes
{
    public const string InvalidRequest = nameof(InvalidRequest);
    public const string InvalidCredentials = nameof(InvalidCredentials);
    public const string InvalidSession = nameof(InvalidSession);
    public const string InternalError = nameof(InternalError);
}
