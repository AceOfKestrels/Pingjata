using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Pingjata.Bot;
using Pingjata.Bot.EventHandlers.CommandHandlers;
using Pingjata.Bot.EventHandlers.MessageHandlers;
using Pingjata.Persistence;
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

    services.AddSingleton(new DiscordSocketClient());
    services.AddHostedService<DiscordBot>();

    services.AddHostedService<PingHandler>();
    services.AddHostedService<DmHandler>();
    services.AddHostedService<PingjataCommandHandler>();

    WebApplication app = builder.Build();

    using (IServiceScope scope = app.Services.CreateScope())
    {
        await using ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    app.Run();
}
catch (Exception e)
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
    string? user = Environment.GetEnvironmentVariable("DB_USER");
    string? password = Environment.GetEnvironmentVariable("DB_PASSWORD");
    string? database = Environment.GetEnvironmentVariable("DB_DATABASE");

    if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password) ||
        string.IsNullOrWhiteSpace(database))
        throw new ArgumentException(
            "The environment variables DB_HOST, DB_USER, DB_PASSWORD and DB_DATABASE must be set");

    string connectionStr = $"Host={host};Username={user};Password={password};Database={database}";

    return connectionStr;
}