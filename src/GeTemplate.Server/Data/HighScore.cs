namespace GeTemplate.Server.Data;

/// <summary>
/// HighScores テーブルのエンティティ。ユーザーごと・ゲームごとのハイスコア。
/// </summary>
public sealed class HighScore
{
    public required string UserId { get; set; }

    public required string GameId { get; set; }

    public int Score { get; set; }

    /// <summary>ハイスコアを更新した日時（UTC）。</summary>
    public DateTime UpdatedAt { get; set; }
}
