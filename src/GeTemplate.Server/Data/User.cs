namespace GeTemplate.Server.Data;

/// <summary>
/// Users テーブルのエンティティ。アカウントとユーザーデータを持つ。
/// </summary>
public sealed class User
{
    /// <summary>ユーザー ID（GUID・32 桁の 16 進数）。</summary>
    public required string Id { get; set; }

    public required string Name { get; set; }

    public int PlayCount { get; set; }

    /// <summary>作成日時（UTC）。</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>最終ログイン日時（UTC）。</summary>
    public DateTime LastLoginAt { get; set; }

    /// <summary>認証トークンの SHA-256（16 進数）。認証トークンそのものは保存しない。</summary>
    public required string AuthTokenHash { get; set; }

    /// <summary>現在のセッショントークン。ログインのたびに置き換わる。</summary>
    public string? SessionToken { get; set; }
}
