using System.Text;
using Discord;
using Discord.WebSocket;
using Pingjata.Extensions;
using Pingjata.ResultPattern;

namespace Pingjata.Bot.EventHandlers.Base;

public abstract class SlashCommandHandler(
    DiscordSocketClient client,
    DiscordBot bot,
    ILogger<SlashCommandHandler> logger) : EventHandler(client)
{
    public const int DescriptionMaxLength = 100;

    protected abstract SlashCommandBuilder Command { get; }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        bot.RegisterSlashCommand(Command);
        Client.SlashCommandExecuted += HandleInternalAsync;

        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        Client.SlashCommandExecuted -= HandleInternalAsync;

        return Task.CompletedTask;
    }

    private Task HandleInternalAsync(SocketSlashCommand command)
    {
        if (command.CommandName != Command.Name)
            return Task.CompletedTask;

        _ = Task.Run(async () =>
        {
            try
            {
                if (command.Channel.IsDm(command.User))
                {
                    await RespondWithError(command, Errors.CommandInDm);

                    return;
                }

                if (command.Channel.IsThread())
                {
                    await RespondWithError(command, Errors.CommandInThread);

                    return;
                }

                await HandleAsync(command);
            }
            catch (Exception e)
            {
                await RespondWithError(command, Errors.CommandInternalError(e));
            }
        });

        return Task.CompletedTask;
    }

    protected abstract Task HandleAsync(SocketSlashCommand command);

    protected virtual Task RespondWithError(SocketSlashCommand command, Error error)
    {
        if (!error.LogMessage.IsEmpty())
            logger.LogWarning("Received invalid command: {Error} {Exception}", error.LogMessage, error.Exception);
        else if (!error.ResponseMessage.IsEmpty())
            logger.LogDebug("Received invalid command: {Error} {Exception}", error.ResponseMessage, error.Exception);

        return command.RespondAsync(error.ResponseMessage, ephemeral: true);
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