namespace GeTemplate.Server.Tests;

/// <summary>
/// サーバを再起動してもデータが残ること（F002-REQ-26）。
/// </summary>
public sealed class PersistenceTests : ServerTestBase
{
    [Fact]
    public async Task AfterRestart_CanLoginAndPlayCountIsKept()
    {
        var (userId, authToken, session) = await Server.CreateAccountAsync();
        await Server.PostAsync("/api/user/add-play-count", sessionToken: session);

        RestartServer();

        var login = await Server.PostAsync("/api/account/login", new { userId, authToken });
        var newSession = login.GetProperty("sessionToken").GetString();
        var user = (await Server.PostAsync("/api/user/fetch", sessionToken: newSession)).GetProperty("user");

        Assert.Equal(userId, user.GetProperty("userId").GetString());
        Assert.Equal(1, user.GetProperty("playCount").GetInt32());
    }
}
