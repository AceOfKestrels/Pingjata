using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Pingjata.Bot;
using Pingjata.Bot.EventHandlers.CommandHandlers;
using Pingjata.Bot.EventHandlers.MessageHandlers;
using Pingjata.Persistence;
using Pingjata.Service;
using Serilog;

try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration));

    IServiceCollection services = builder.Services;

    string connectionString = GetDbConnectionString();
    services.AddDbContextFactory<ApplicationDbContext>(options => { options.UseNpgsql(connectionString); });

    services.AddSingleton<CounterService>();

    services.AddSingleton(new DiscordSocketClient());

    services.AddSingleton<DiscordBot>();
    services.AddHostedService(p => p.GetRequiredService<DiscordBot>());

    services.AddHostedService<AutoResetService>();

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

    await ConnectToDatabase(app.Services);

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

async Task ConnectToDatabase(IServiceProvider serviceProvider)
{
    using IServiceScope scope = serviceProvider.CreateScope();
    await using ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    ILogger<Program> logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    if (!int.TryParse(Environment.GetEnvironmentVariable("DB_CONNECTION_ATTEMPTS"), out int limit))
        limit = 10;

    for (int i = 1; i <= limit; i++)
    {
        try
        {
            logger.LogInformation("Trying to connect to database...");
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Connected to database.");

            return;
        }
        catch (Exception)
        {
            if (i == limit)
                throw;

            const int retrySeconds = 5;
            logger.LogInformation("Failed to connect to database. Retrying in {Seconds} seconds...", retrySeconds);
            await Task.Delay(TimeSpan.FromSeconds(retrySeconds));
        }
    }
}