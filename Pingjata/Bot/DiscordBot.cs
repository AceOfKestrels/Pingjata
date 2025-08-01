using Discord;
using Discord.WebSocket;

namespace Pingjata.Bot;

public class DiscordBot(DiscordSocketClient client, ILogger<DiscordBot> logger): IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        client.Log += Log;
        
        string? botToken = Environment.GetEnvironmentVariable("BOT_TOKEN");
        if (string.IsNullOrWhiteSpace(botToken))
            throw new ArgumentException("BOT_TOKEN environment variable must be set");

        try
        {
            await client.LoginAsync(TokenType.Bot, botToken);
            await client.StartAsync();
        }
        catch (Exception e)
        {
            throw new AggregateException("Could not log in to bot account", e);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private Task Log(LogMessage message)
    {
        switch (message.Severity)
        {
            case LogSeverity.Debug:
                logger.LogDebug("{Source}: {Message}", message.Source, message.Message);
                break;
            case LogSeverity.Warning:
                logger.LogWarning("{Source}: {Message}", message.Source, message.Message);
                break;
            case LogSeverity.Error:
                logger.LogError("{Source}: {Message} - {Error}", message.Source, message.Message, message.Exception);
                break;
            case LogSeverity.Critical:
                logger.LogCritical("{Source}: {Message} - {Error}", message.Source, message.Message, message.Exception);
                break;
            case LogSeverity.Verbose:
            case LogSeverity.Info:
            default:
                logger.LogInformation("{Source}: {Message}", message.Source, message.Message);
                break;
        }
        return Task.CompletedTask;
    }
}