using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GeTemplate.Server.Tests;

/// <summary>
/// 一時ファイルの SQLite と、手で進められる時計を使うテスト用サーバ。
/// 同じ DbPath で作り直すと、サーバの再起動に相当する。
/// </summary>
public sealed class TestServer : WebApplicationFactory<Program>
{
    public TestServer(string dbPath, ManualTimeProvider time)
    {
        DbPath = dbPath;
        Time = time;
    }

    public string DbPath { get; }
    public ManualTimeProvider Time { get; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:GameDb", $"Data Source={DbPath}");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Time);
        });
    }

    public static string NewDbPath() => Path.Combine(Path.GetTempPath(), $"ge_template_test_{Guid.NewGuid():N}.db");

    public static void DeleteDb(string dbPath)
    {
        // 接続プールがファイルを開いたままにするため、先に閉じる。
        SqliteConnection.ClearAllPools();
        File.Delete(dbPath);
    }

    public async Task<JsonElement> PostAsync(string path, object? body = null, string? sessionToken = null, int expectedStatus = 200)
    {
        using var client = CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(body ?? new { }),
        };
        if (sessionToken is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", sessionToken);
        }

        using var response = await client.SendAsync(request);
        var text = await response.Content.ReadAsStringAsync();
        Assert.True((int)response.StatusCode == expectedStatus, $"HTTP {(int)response.StatusCode}（期待値 {expectedStatus}）: {text}");
        return JsonDocument.Parse(text).RootElement.Clone();
    }

    public async Task<JsonElement> PostRawAsync(string path, string json, int expectedStatus)
    {
        using var client = CreateClient();
        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        using var response = await client.PostAsync(path, content);
        var text = await response.Content.ReadAsStringAsync();
        Assert.True((int)response.StatusCode == expectedStatus, $"HTTP {(int)response.StatusCode}（期待値 {expectedStatus}）: {text}");
        return JsonDocument.Parse(text).RootElement.Clone();
    }

    /// <summary>アカウントを作成し、(userId, authToken, sessionToken) を返す。</summary>
    public async Task<(string UserId, string AuthToken, string SessionToken)> CreateAccountAsync()
    {
        var json = await PostAsync("/api/account/create");
        return (json.GetProperty("userId").GetString()!, json.GetProperty("authToken").GetString()!, json.GetProperty("sessionToken").GetString()!);
    }
}

/// <summary>
/// テストで現在時刻を固定・変更するための TimeProvider。
/// </summary>
public sealed class ManualTimeProvider(DateTimeOffset now) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = now;

    public override DateTimeOffset GetUtcNow() => Now;
}

/// <summary>
/// テストごとに一時 DB のサーバを用意し、終わったら消す基底クラス。
/// </summary>
public abstract class ServerTestBase : IDisposable
{
    protected static readonly DateTimeOffset StartTime = new(2026, 10, 20, 1, 23, 45, 678, TimeSpan.Zero);

    protected ServerTestBase()
    {
        DbPath = TestServer.NewDbPath();
        Time = new ManualTimeProvider(StartTime);
        Server = new TestServer(DbPath, Time);
    }

    protected string DbPath { get; }
    protected ManualTimeProvider Time { get; }
    protected TestServer Server { get; private set; }

    /// <summary>同じ DB ファイルでサーバを作り直す（サーバの再起動に相当）。</summary>
    protected void RestartServer()
    {
        Server.Dispose();
        Server = new TestServer(DbPath, Time);
    }

    public void Dispose()
    {
        Server.Dispose();
        TestServer.DeleteDb(DbPath);
        GC.SuppressFinalize(this);
    }
}
