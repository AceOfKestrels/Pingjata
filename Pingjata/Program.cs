using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Pingjata.Bot;
using Pingjata.Bot.EventHandlers.CommandHandlers;
using Pingjata.Bot.EventHandlers.MessageHandlers;
using Pingjata.Persistence;
using Pingjata.Service;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    IServiceCollection services = builder.Services;

    services.AddSerilog();

    string connectionString = GetDbConnectionString();
    services.AddDbContextFactory<ApplicationDbContext>(options =>
    {
        options.UseNpgsql(connectionString);
    });

    services.AddSingleton<CounterService>();

    services.AddSingleton(new DiscordSocketClient());

    services.AddSingleton<DiscordBot>();
    services.AddHostedService(p => p.GetRequiredService<DiscordBot>());

    services.AddHostedService<PingHandler>();
    services.AddHostedService<DmHandler>();

    services.AddHostedService<HelpCommandHandler>();
    services.AddHostedService<PauseCommandHandler>();
    services.AddHostedService<ResetCommandHandler>();
    services.AddHostedService<StartCommandHandler>();
    services.AddHostedService<StatusCommandHandler>();
    services.AddHostedService<StopCommandHandler>();
    services.AddHostedService<UnpauseCommandHandler>();

    services.AddSingleton<PingService>();
    services.AddHostedService(p => p.GetRequiredService<PingService>());

    WebApplication app = builder.Build();

    using (IServiceScope scope = app.Services.CreateScope())
    {
        await using ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    app.Run();
}
catch (Exception e) when (e is not HostAbortedException)
{
    Log.Fatal(e, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

return;

string GetDbConnectionString()
{
    string? host = Environment.GetEnvironmentVariable("DB_HOST");
    string port = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
    string? user = Environment.GetEnvironmentVariable("DB_USER");
    string? password = Environment.GetEnvironmentVariable("DB_PASSWORD");
    string? database = Environment.GetEnvironmentVariable("DB_DATABASE");

    if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password) ||
        string.IsNullOrWhiteSpace(database))
        throw new ArgumentException(
            "The environment variables DB_HOST, DB_USER, DB_PASSWORD and DB_DATABASE must be set");

    string connectionStr = $"Host={host}:{port};Username={user};Password={password};Database={database}";

    return connectionStr;
}