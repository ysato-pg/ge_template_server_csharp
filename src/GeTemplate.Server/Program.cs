using GeTemplate.Server.Account;
using GeTemplate.Server.Common;
using GeTemplate.Server.Data;
using GeTemplate.Server.User;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseSqlite(ResolveConnectionString(builder.Configuration, builder.Environment)));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<UserService>();

var app = builder.Build();

// 起動時にマイグレーションを適用する（DB ファイルが無ければ作る）。
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<GameDbContext>().Database.Migrate();
}

app.UseApiErrorHandling();
app.MapAccountEndpoints();
app.MapUserEndpoints();

app.Run();

// SQLite の相対パスは、実行したフォルダではなくプロジェクトのフォルダ（ContentRoot）を基準にする。
static string ResolveConnectionString(IConfiguration configuration, IWebHostEnvironment environment)
{
    var connectionString = configuration.GetConnectionString("GameDb")
        ?? throw new InvalidOperationException("ConnectionStrings:GameDb が設定されていません。");
    var sqlite = new SqliteConnectionStringBuilder(connectionString);
    if (!Path.IsPathRooted(sqlite.DataSource))
    {
        sqlite.DataSource = Path.Combine(environment.ContentRootPath, sqlite.DataSource);
    }
    Directory.CreateDirectory(Path.GetDirectoryName(sqlite.DataSource)!);
    return sqlite.ToString();
}

// テスト（WebApplicationFactory）から参照するため。
public partial class Program;
