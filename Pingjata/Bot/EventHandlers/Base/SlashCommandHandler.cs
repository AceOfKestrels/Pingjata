using System.Text;
using Discord;
using Discord.WebSocket;

namespace Pingjata.Bot.EventHandlers.Base;

public abstract class SlashCommandHandler(DiscordSocketClient client, ILogger<SlashCommandHandler> logger) : EventHandler(client)
{
    public const int DescriptionMaxLength = 100;
    protected abstract SlashCommandBuilder Command { get; }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        Client.Ready += RegisterCommand;
        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        Client.SlashCommandExecuted -= HandleInternalAsync;
        Client.Ready -= RegisterCommand;
        return Task.CompletedTask;
    }

    private async Task RegisterCommand()
    {
        try
        {
            await Client.CreateGlobalApplicationCommandAsync(Command.Build());
        }
        catch (Exception e)
        {
            logger.LogError("Failed to initialize command: {Error}", e);
        }

        Client.SlashCommandExecuted += HandleInternalAsync;
    }

    private Task HandleInternalAsync(SocketSlashCommand command)
    {
        if (command.CommandName != Command.Name)
            return Task.CompletedTask;

        _ = Task.Run(async () =>
        {
            try
            {
                await HandleAsync(command);
            }
            catch (Exception e)
            {
                logger.LogError("Error while executing command: {Error}", e);
                await command.RespondAsync("An error occurred while executing this command.", ephemeral: true);
            }
        });
        return Task.CompletedTask;
    }

    protected abstract Task HandleAsync(SocketSlashCommand command);

    protected virtual Task RespondWithError(SocketSlashCommand command, string error)
    {
        logger.LogWarning("Received invalid command: {Error}", error);

        return command.RespondAsync(error, ephemeral: true);
    }
    
    protected readonly struct Description
    {
        private Description(string value)
        {
            StringBuilder builder = new();
            int max = Math.Min(value.Length, DescriptionMaxLength);
            for (int i = 0; i < max; i++)
                builder.Append(value[i]);

            Value = builder.ToString();
        }

        private string Value { get; } 

        public static implicit operator string(Description desc) => desc.Value;
        public static implicit operator Description(string desc) => new(desc);
    }
}