namespace GeTemplate.Server.InGame;

using System.Text.RegularExpressions;

/// <summary>
/// ゲーム ID の形式。英小文字・数字・_ の 1〜32 文字。クライアントの GameDefinition.IsValidGameId と同じ。
/// </summary>
public static partial class GameIds
{
    public static bool IsValid(string? gameId) => gameId is not null && Pattern().IsMatch(gameId);

    [GeneratedRegex("^[a-z0-9_]{1,32}$")]
    private static partial Regex Pattern();
}
