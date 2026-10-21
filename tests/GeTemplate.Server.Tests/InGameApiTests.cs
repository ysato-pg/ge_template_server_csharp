using System.Text.Json;

namespace GeTemplate.Server.Tests;

/// <summary>
/// /api/ingame/*（F003-REQ-13, 16〜18。F002-REQ-22 のプレイ回数の加算）。
/// </summary>
public sealed class InGameApiTests : ServerTestBase
{
    [Fact]
    public async Task Start_IncrementsPlayCountAndIssuesPlayId()
    {
        var (_, _, session) = await Server.CreateAccountAsync();

        var first = await StartAsync(session, "sample");
        var second = await StartAsync(session, "sample");
        var fetched = (await Server.PostAsync("/api/user/fetch", sessionToken: session)).GetProperty("user");

        Assert.Equal(1, first.GetProperty("user").GetProperty("playCount").GetInt32());
        Assert.Equal(2, second.GetProperty("user").GetProperty("playCount").GetInt32());
        Assert.Equal(2, fetched.GetProperty("playCount").GetInt32());
        Assert.Matches("^[0-9a-f]{32}$", first.GetProperty("playId").GetString());
        Assert.NotEqual(first.GetProperty("playId").GetString(), second.GetProperty("playId").GetString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Sample")]
    [InlineData("sample-game")]
    [InlineData("a23456789012345678901234567890123")]
    public async Task Start_WithInvalidGameId_ReturnsInvalidRequest(string? gameId)
    {
        var (_, _, session) = await Server.CreateAccountAsync();

        var error = await Server.PostAsync("/api/ingame/start", new { gameId }, session, expectedStatus: 400);

        Assert.Equal("InvalidRequest", error.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task End_UpdatesHighScoreOnlyWhenHigher()
    {
        var (_, _, session) = await Server.CreateAccountAsync();

        var r1 = await PlayAsync(session, "sample", "clear", 100);
        var r2 = await PlayAsync(session, "sample", "clear", 50);
        var r3 = await PlayAsync(session, "sample", "clear", 150);

        AssertEnd(r1, "sample", 100, true);
        AssertEnd(r2, "sample", 100, false);
        AssertEnd(r3, "sample", 150, true);
    }

    [Fact]
    public async Task End_FailCountsAndRetireDoesNot()
    {
        var (_, _, session) = await Server.CreateAccountAsync();

        var retire = await PlayAsync(session, "sample", "retire", 1000);
        var fail = await PlayAsync(session, "sample", "fail", 200);
        var retireAgain = await PlayAsync(session, "sample", "retire", 1000);

        AssertEnd(retire, "sample", 0, false);
        AssertEnd(fail, "sample", 200, true);
        AssertEnd(retireAgain, "sample", 200, false);
    }

    [Fact]
    public async Task End_FirstScoreZeroIsNewHighScore()
    {
        var (_, _, session) = await Server.CreateAccountAsync();

        AssertEnd(await PlayAsync(session, "sample", "clear", 0), "sample", 0, true);
    }

    [Fact]
    public async Task HighScores_AreSeparatedByGameAndUser()
    {
        var (_, _, sessionA) = await Server.CreateAccountAsync();
        var (_, _, sessionB) = await Server.CreateAccountAsync();

        await PlayAsync(sessionA, "game_a", "clear", 300);
        var gameB = await PlayAsync(sessionA, "game_b", "clear", 10);
        var otherUser = await PlayAsync(sessionB, "game_a", "clear", 5);

        AssertEnd(gameB, "game_b", 10, true);
        AssertEnd(otherUser, "game_a", 5, true);
    }

    [Fact]
    public async Task End_Twice_ReturnsInvalidPlayAndKeepsHighScore()
    {
        var (_, _, session) = await Server.CreateAccountAsync();
        var playId = (await StartAsync(session, "sample")).GetProperty("playId").GetString();
        await EndAsync(session, playId, "sample", "clear", 10);

        var error = await EndAsync(session, playId, "sample", "clear", 999, expectedStatus: 409);
        var next = await PlayAsync(session, "sample", "clear", 1);

        Assert.Equal("InvalidPlay", error.GetProperty("errorCode").GetString());
        AssertEnd(next, "sample", 10, false);
    }

    [Fact]
    public async Task End_WithoutStart_ReturnsInvalidPlay()
    {
        var (_, _, session) = await Server.CreateAccountAsync();

        var error = await EndAsync(session, "0123456789abcdef0123456789abcdef", "sample", "clear", 10, expectedStatus: 409);

        Assert.Equal("InvalidPlay", error.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task End_WithDifferentGameId_ReturnsInvalidPlay()
    {
        var (_, _, session) = await Server.CreateAccountAsync();
        var playId = (await StartAsync(session, "sample")).GetProperty("playId").GetString();

        var error = await EndAsync(session, playId, "other", "clear", 10, expectedStatus: 409);

        Assert.Equal("InvalidPlay", error.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task End_WithReplacedPlayId_ReturnsInvalidPlay()
    {
        var (_, _, session) = await Server.CreateAccountAsync();
        var oldPlayId = (await StartAsync(session, "sample")).GetProperty("playId").GetString();
        await StartAsync(session, "sample");

        var error = await EndAsync(session, oldPlayId, "sample", "clear", 10, expectedStatus: 409);

        Assert.Equal("InvalidPlay", error.GetProperty("errorCode").GetString());
    }

    [Theory]
    [InlineData("""{ "gameId": "sample", "result": "clear", "score": -1 }""")]
    [InlineData("""{ "gameId": "sample", "result": "clear" }""")]
    [InlineData("""{ "gameId": "sample", "score": 10 }""")]
    [InlineData("""{ "gameId": "sample", "result": "win", "score": 10 }""")]
    [InlineData("""{ "gameId": "sample", "result": 0, "score": 10 }""")]
    [InlineData("""{ "result": "clear", "score": 10 }""")]
    public async Task End_WithInvalidBody_ReturnsInvalidRequest(string bodyWithoutPlayId)
    {
        var (_, _, session) = await Server.CreateAccountAsync();
        var playId = (await StartAsync(session, "sample")).GetProperty("playId").GetString();
        var body = "{ \"playId\": \"" + playId + "\", " + bodyWithoutPlayId.TrimStart('{');

        var error = await Server.PostRawAsync("/api/ingame/end", body, 400, session);

        Assert.Equal("InvalidRequest", error.GetProperty("errorCode").GetString());
    }

    [Theory]
    [InlineData("/api/ingame/start")]
    [InlineData("/api/ingame/end")]
    public async Task WithoutSession_ReturnsInvalidSession(string path)
    {
        var error = await Server.PostAsync(path, expectedStatus: 401);
        Assert.Equal("InvalidSession", error.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task AfterRestart_HighScoreIsKept()
    {
        var (_, _, session) = await Server.CreateAccountAsync();
        await PlayAsync(session, "sample", "clear", 100);

        RestartServer();

        AssertEnd(await PlayAsync(session, "sample", "clear", 50), "sample", 100, false);
    }

    private Task<JsonElement> StartAsync(string session, string gameId) =>
        Server.PostAsync("/api/ingame/start", new { gameId }, session);

    private Task<JsonElement> EndAsync(string session, string? playId, string gameId, string result, int score, int expectedStatus = 200) =>
        Server.PostAsync("/api/ingame/end", new { playId, gameId, result, score }, session, expectedStatus);

    private async Task<JsonElement> PlayAsync(string session, string gameId, string result, int score)
    {
        var playId = (await StartAsync(session, gameId)).GetProperty("playId").GetString();
        return await EndAsync(session, playId, gameId, result, score);
    }

    private static void AssertEnd(JsonElement json, string gameId, int highScore, bool isNewHighScore)
    {
        Assert.Equal(gameId, json.GetProperty("gameId").GetString());
        Assert.Equal(highScore, json.GetProperty("highScore").GetInt32());
        Assert.Equal(isNewHighScore, json.GetProperty("isNewHighScore").GetBoolean());
    }
}
