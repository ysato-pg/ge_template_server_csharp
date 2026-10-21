namespace GeTemplate.Server.InGame;

using GeTemplate.Server.Common;

/// <summary>
/// インゲームの結果。JSON では "clear"・"fail"・"retire"（Program で camelCase の文字列にする）。
/// </summary>
public enum PlayResult
{
    Clear,
    Fail,
    Retire,
}

public sealed record StartInGameRequest(string? GameId);

public sealed record StartInGameResponse(string PlayId, UserDataDto User);

public sealed record EndInGameRequest(string? PlayId, string? GameId, PlayResult? Result, int? Score);

public sealed record EndInGameResponse(string GameId, int HighScore, bool IsNewHighScore);
