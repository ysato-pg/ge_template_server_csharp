namespace GeTemplate.Server.Common;

using GeTemplate.Server.Data;

/// <summary>
/// レスポンスの UserData。
/// </summary>
public sealed record UserDataDto(string UserId, string Name, int PlayCount, DateTime CreatedAt, DateTime LastLoginAt)
{
    public static UserDataDto From(User user) =>
        new(user.Id, user.Name, user.PlayCount, user.CreatedAt, user.LastLoginAt);
}

/// <summary>
/// ユーザーデータを返す API のレスポンス { "user": UserData }。
/// </summary>
public sealed record UserResponse(UserDataDto User);
