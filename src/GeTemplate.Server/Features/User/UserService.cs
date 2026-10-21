namespace GeTemplate.Server.User;

using GeTemplate.Server.Common;
using GeTemplate.Server.Data;

/// <summary>
/// 更新取得・取得（プレイ回数の加算はインゲーム開始で行う）。クライアントの疑似サーバ（MockServerLogic）と同じ規則にする。
/// 対象のユーザーは SessionAuth で認証済みのもの。
/// </summary>
public sealed class UserService(GameDbContext db, TimeProvider time)
{
    /// <summary>更新取得。最終ログイン日時を現在時刻にして返す。</summary>
    public async Task<UserResponse> UpdateAndFetchAsync(User user, CancellationToken cancellationToken)
    {
        user.LastLoginAt = time.UtcNowSeconds();
        await db.SaveChangesAsync(cancellationToken);
        return new UserResponse(UserDataDto.From(user));
    }

    /// <summary>取得。更新はしない。</summary>
    public UserResponse Fetch(User user) => new(UserDataDto.From(user));
}
