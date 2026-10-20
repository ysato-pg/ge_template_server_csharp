namespace GeTemplate.Server.Common;

/// <summary>
/// 現在時刻の取得。秒未満を切り捨てた UTC にする（JSON・DB・疑似サーバで同じ値になるように）。
/// </summary>
public static class Clock
{
    public static DateTime UtcNowSeconds(this TimeProvider time)
    {
        var now = time.GetUtcNow().UtcDateTime;
        return new DateTime(now.Ticks - now.Ticks % TimeSpan.TicksPerSecond, DateTimeKind.Utc);
    }
}
