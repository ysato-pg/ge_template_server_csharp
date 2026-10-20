namespace GeTemplate.Server.Tests;

/// <summary>
/// /api/account/*（F002-REQ-11, 14, 15, 21）。
/// </summary>
public sealed class AccountApiTests : ServerTestBase
{
    [Fact]
    public async Task Create_ReturnsIdsAndTokens_WithInitialName()
    {
        var (userId, authToken, sessionToken) = await Server.CreateAccountAsync();

        Assert.Matches("^[0-9a-f]{32}$", userId);
        Assert.Matches("^[A-Za-z0-9_-]{43}$", authToken);
        Assert.Matches("^[A-Za-z0-9_-]{43}$", sessionToken);

        var user = (await Server.PostAsync("/api/user/fetch", sessionToken: sessionToken)).GetProperty("user");
        Assert.Equal(userId, user.GetProperty("userId").GetString());
        Assert.Equal("プレイヤー", user.GetProperty("name").GetString());
        Assert.Equal(0, user.GetProperty("playCount").GetInt32());
    }

    [Fact]
    public async Task Create_Twice_IssuesDifferentUserIds()
    {
        var first = await Server.CreateAccountAsync();
        var second = await Server.CreateAccountAsync();

        Assert.NotEqual(first.UserId, second.UserId);
    }

    [Fact]
    public async Task Login_WithSavedCredentials_ReturnsSessionForSameUser()
    {
        var (userId, authToken, _) = await Server.CreateAccountAsync();

        var login = await Server.PostAsync("/api/account/login", new { userId, authToken });
        var sessionToken = login.GetProperty("sessionToken").GetString()!;

        var user = (await Server.PostAsync("/api/user/fetch", sessionToken: sessionToken)).GetProperty("user");
        Assert.Equal(userId, user.GetProperty("userId").GetString());
    }

    [Fact]
    public async Task Login_InvalidatesPreviousSession()
    {
        var (userId, authToken, oldSession) = await Server.CreateAccountAsync();

        await Server.PostAsync("/api/account/login", new { userId, authToken });

        var error = await Server.PostAsync("/api/user/fetch", sessionToken: oldSession, expectedStatus: 401);
        Assert.Equal("InvalidSession", error.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task Login_WithWrongAuthToken_ReturnsInvalidCredentials()
    {
        var (userId, _, _) = await Server.CreateAccountAsync();

        var error = await Server.PostAsync("/api/account/login", new { userId, authToken = "wrong" }, expectedStatus: 401);
        Assert.Equal("InvalidCredentials", error.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task Login_WithUnknownUser_ReturnsInvalidCredentials()
    {
        var error = await Server.PostAsync("/api/account/login", new { userId = "unknown", authToken = "token" }, expectedStatus: 401);
        Assert.Equal("InvalidCredentials", error.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task Login_WithMissingFields_ReturnsInvalidRequest()
    {
        var error = await Server.PostAsync("/api/account/login", new { }, expectedStatus: 400);
        Assert.Equal("InvalidRequest", error.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task Login_WithMalformedJson_ReturnsInvalidRequest()
    {
        var error = await Server.PostRawAsync("/api/account/login", "{ not json", expectedStatus: 400);
        Assert.Equal("InvalidRequest", error.GetProperty("errorCode").GetString());
    }
}
