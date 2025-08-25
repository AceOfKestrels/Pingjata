using Discord;
using Discord.WebSocket;

namespace Pingjata.Bot;

public class DiscordBot(DiscordSocketClient client, ILogger<DiscordBot> logger) : IHostedService
{
    private readonly Dictionary<string, ApplicationCommandProperties> Commands = [];
    private bool _ready;
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        client.Log += Log;
        client.Ready += OnReady;

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

    public void RegisterSlashCommand(SlashCommandBuilder command)
    {
        if (_ready)
        {
            logger.LogWarning("Trying to register command while bot is already in 'ready' state");

            return;
        }

        if (Commands.ContainsKey(command.Name))
        {
            logger.LogWarning("Command with name {Name} already registered", command.Name);

            return;
        }

        Commands.Add(command.Name, command.Build());
    }

    private async Task OnReady()
    {
        _ready = true;

        try
        {
            await client.BulkOverwriteGlobalApplicationCommandsAsync([..Commands.Values]);
        }
        catch (Exception e)
        {
            logger.LogError("Failed to create application commands: {Error}", e);
        }
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