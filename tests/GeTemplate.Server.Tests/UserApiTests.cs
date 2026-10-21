namespace GeTemplate.Server.Tests;

/// <summary>
/// /api/user/*（F002-REQ-11, 17, 18）。プレイ回数の加算（F002-REQ-22）は InGameApiTests。
/// </summary>
public sealed class UserApiTests : ServerTestBase
{
    [Fact]
    public async Task UpdateAndFetch_UpdatesLastLoginAtToNow()
    {
        var (_, _, session) = await Server.CreateAccountAsync();
        Time.Now = StartTime.AddHours(5);

        var user = (await Server.PostAsync("/api/user/update-and-fetch", sessionToken: session)).GetProperty("user");

        // 秒未満は切り捨てる。
        Assert.Equal("2026-10-20T06:23:45Z", user.GetProperty("lastLoginAt").GetString());
        Assert.Equal("2026-10-20T01:23:45Z", user.GetProperty("createdAt").GetString());
    }

    [Fact]
    public async Task Fetch_DoesNotUpdateLastLoginAt()
    {
        var (_, _, session) = await Server.CreateAccountAsync();
        Time.Now = StartTime.AddHours(5);

        var user = (await Server.PostAsync("/api/user/fetch", sessionToken: session)).GetProperty("user");

        Assert.Equal("2026-10-20T01:23:45Z", user.GetProperty("lastLoginAt").GetString());
    }

    [Theory]
    [InlineData("/api/user/update-and-fetch")]
    [InlineData("/api/user/fetch")]
    public async Task WithoutSession_ReturnsInvalidSession(string path)
    {
        var error = await Server.PostAsync(path, expectedStatus: 401);
        Assert.Equal("InvalidSession", error.GetProperty("errorCode").GetString());
    }

    [Theory]
    [InlineData("/api/user/update-and-fetch")]
    [InlineData("/api/user/fetch")]
    public async Task WithWrongSession_ReturnsInvalidSession(string path)
    {
        await Server.CreateAccountAsync();

        var error = await Server.PostAsync(path, sessionToken: "wrong", expectedStatus: 401);
        Assert.Equal("InvalidSession", error.GetProperty("errorCode").GetString());
    }
}
