namespace GeTemplate.Server.Tests;

/// <summary>
/// /api/user/*（F002-REQ-11, 17, 18, 22）。
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

    [Fact]
    public async Task AddPlayCount_IncrementsByOne()
    {
        var (_, _, session) = await Server.CreateAccountAsync();

        var first = (await Server.PostAsync("/api/user/add-play-count", sessionToken: session)).GetProperty("user");
        var second = (await Server.PostAsync("/api/user/add-play-count", sessionToken: session)).GetProperty("user");
        var fetched = (await Server.PostAsync("/api/user/fetch", sessionToken: session)).GetProperty("user");

        Assert.Equal(1, first.GetProperty("playCount").GetInt32());
        Assert.Equal(2, second.GetProperty("playCount").GetInt32());
        Assert.Equal(2, fetched.GetProperty("playCount").GetInt32());
    }

    [Theory]
    [InlineData("/api/user/update-and-fetch")]
    [InlineData("/api/user/fetch")]
    [InlineData("/api/user/add-play-count")]
    public async Task WithoutSession_ReturnsInvalidSession(string path)
    {
        var error = await Server.PostAsync(path, expectedStatus: 401);
        Assert.Equal("InvalidSession", error.GetProperty("errorCode").GetString());
    }

    [Theory]
    [InlineData("/api/user/update-and-fetch")]
    [InlineData("/api/user/fetch")]
    [InlineData("/api/user/add-play-count")]
    public async Task WithWrongSession_ReturnsInvalidSession(string path)
    {
        await Server.CreateAccountAsync();

        var error = await Server.PostAsync(path, sessionToken: "wrong", expectedStatus: 401);
        Assert.Equal("InvalidSession", error.GetProperty("errorCode").GetString());
    }
}
