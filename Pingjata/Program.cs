using Discord.WebSocket;
using Pingjata.Bot;
using Pingjata.Bot.EventHandlers.CommandHandlers;
using Pingjata.Bot.EventHandlers.MessageHandlers;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    IServiceCollection services = builder.Services;

    services.AddSerilog();

    services.AddSingleton(new DiscordSocketClient());
    services.AddHostedService<DiscordBot>();

    services.AddHostedService<PingHandler>();
    services.AddHostedService<DmHandler>();
    services.AddHostedService<PingjataCommandHandler>();

    WebApplication app = builder.Build();

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