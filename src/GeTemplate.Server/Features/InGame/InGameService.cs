namespace GeTemplate.Server.InGame;

using GeTemplate.Server.Common;
using GeTemplate.Server.Data;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// インゲーム開始・終了。クライアントの疑似サーバ（MockServerLogic）と同じ規則にする。
/// 対象のユーザーは SessionAuth で認証済みのもの。
/// </summary>
public sealed class InGameService(GameDbContext db, TimeProvider time)
{
    /// <summary>インゲーム開始。プレイ回数を 1 加算し、プレイ ID を発行する（進行中のプレイがあれば置き換える）。</summary>
    public async Task<StartInGameResponse> StartAsync(User user, StartInGameRequest request, CancellationToken cancellationToken)
    {
        if (!GameIds.IsValid(request.GameId))
        {
            throw ApiException.InvalidRequest("gameId の形式が正しくありません。");
        }

        user.PlayCount++;
        user.CurrentPlayId = Guid.NewGuid().ToString("N");
        user.CurrentPlayGameId = request.GameId;
        await db.SaveChangesAsync(cancellationToken);
        return new StartInGameResponse(user.CurrentPlayId, UserDataDto.From(user));
    }

    /// <summary>インゲーム終了。リタイア以外で、記録が無いかスコアが上回ればハイスコアを更新する。</summary>
    public async Task<EndInGameResponse> EndAsync(User user, EndInGameRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.PlayId) || !GameIds.IsValid(request.GameId) || request.Result is null || request.Score is null or < 0)
        {
            throw ApiException.InvalidRequest("playId・gameId・result・score（0 以上）が必要です。");
        }
        if (user.CurrentPlayId != request.PlayId || user.CurrentPlayGameId != request.GameId)
        {
            throw ApiException.InvalidPlay();
        }

        var gameId = request.GameId!;
        var score = request.Score.Value;
        user.CurrentPlayId = null;
        user.CurrentPlayGameId = null;

        var highScore = await db.HighScores.SingleOrDefaultAsync(h => h.UserId == user.Id && h.GameId == gameId, cancellationToken);
        var isNew = false;
        if (request.Result != PlayResult.Retire && (highScore is null || score > highScore.Score))
        {
            if (highScore is null)
            {
                highScore = new HighScore { UserId = user.Id, GameId = gameId };
                db.HighScores.Add(highScore);
            }
            highScore.Score = score;
            highScore.UpdatedAt = time.UtcNowSeconds();
            isNew = true;
        }

        await db.SaveChangesAsync(cancellationToken);
        return new EndInGameResponse(gameId, highScore?.Score ?? 0, isNew);
    }
}
