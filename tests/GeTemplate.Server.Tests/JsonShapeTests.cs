using System.Text.Json;

namespace GeTemplate.Server.Tests;

/// <summary>
/// 応答の JSON の形が設計書の JSON 例と同じであること（F002-REQ-04, 20）。
/// クライアントの ServerJson のテストも同じ JSON 例を使う。
/// </summary>
public sealed class JsonShapeTests : ServerTestBase
{
    [Fact]
    public async Task CreateAccount_HasDesignedProperties()
    {
        var json = await Server.PostAsync("/api/account/create");

        AssertProperties(json, "userId", "authToken", "sessionToken");
    }

    [Fact]
    public async Task Login_HasDesignedProperties()
    {
        var (userId, authToken, _) = await Server.CreateAccountAsync();

        var json = await Server.PostAsync("/api/account/login", new { userId, authToken });

        AssertProperties(json, "sessionToken");
    }

    [Fact]
    public async Task UserData_HasDesignedPropertiesAndFormats()
    {
        var (_, _, session) = await Server.CreateAccountAsync();

        var json = await Server.PostAsync("/api/user/fetch", sessionToken: session);

        AssertProperties(json, "user");
        var user = json.GetProperty("user");
        AssertProperties(user, "userId", "name", "playCount", "createdAt", "lastLoginAt");
        Assert.Equal(JsonValueKind.String, user.GetProperty("userId").ValueKind);
        Assert.Equal(JsonValueKind.String, user.GetProperty("name").ValueKind);
        Assert.Equal(JsonValueKind.Number, user.GetProperty("playCount").ValueKind);
        // 日時は UTC の ISO 8601（秒まで、末尾 Z）。
        Assert.Matches(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z$", user.GetProperty("createdAt").GetString());
        Assert.Matches(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z$", user.GetProperty("lastLoginAt").GetString());
    }

    [Fact]
    public async Task Error_HasDesignedProperties()
    {
        var json = await Server.PostAsync("/api/user/fetch", expectedStatus: 401);

        AssertProperties(json, "errorCode", "message");
    }

    private static void AssertProperties(JsonElement json, params string[] expected)
    {
        var actual = json.EnumerateObject().Select(p => p.Name).OrderBy(n => n, StringComparer.Ordinal);
        Assert.Equal(expected.OrderBy(n => n, StringComparer.Ordinal), actual);
    }
}
