namespace GeTemplate.Server.Account;

using GeTemplate.Server.Common;
using GeTemplate.Server.Data;
using Microsoft.EntityFrameworkCore;

public sealed record CreateAccountResponse(string UserId, string AuthToken, string SessionToken);

public sealed record LoginRequest(string? UserId, string? AuthToken);

public sealed record LoginResponse(string SessionToken);

/// <summary>
/// アカウント作成・ログイン。クライアントの疑似サーバ（MockServerLogic）と同じ規則にする。
/// </summary>
public sealed class AccountService(GameDbContext db, TimeProvider time)
{
    /// <summary>アカウント作成時の名前（固定の初期名）。</summary>
    public const string InitialName = "プレイヤー";

    public async Task<CreateAccountResponse> CreateAsync(CancellationToken cancellationToken)
    {
        var now = time.UtcNowSeconds();
        var authToken = TokenGenerator.NewToken();
        var user = new User
        {
            Id = TokenGenerator.NewUserId(),
            Name = InitialName,
            PlayCount = 0,
            CreatedAt = now,
            LastLoginAt = now,
            AuthTokenHash = TokenGenerator.Hash(authToken),
            SessionToken = TokenGenerator.NewToken(),
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        return new CreateAccountResponse(user.Id, authToken, user.SessionToken);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.AuthToken))
        {
            throw ApiException.InvalidRequest("userId と authToken が必要です。");
        }

        var user = await db.Users.SingleOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user is null || user.AuthTokenHash != TokenGenerator.Hash(request.AuthToken))
        {
            throw ApiException.InvalidCredentials();
        }

        // 新しいセッショントークンを発行し、前のものは無効にする。
        user.SessionToken = TokenGenerator.NewToken();
        await db.SaveChangesAsync(cancellationToken);
        return new LoginResponse(user.SessionToken);
    }
}
