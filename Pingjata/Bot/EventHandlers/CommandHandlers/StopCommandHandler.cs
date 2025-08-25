using Discord;
using Discord.WebSocket;
using Pingjata.Bot.EventHandlers.Base;
using Pingjata.Extensions;

namespace Pingjata.Bot.EventHandlers.CommandHandlers;

public class StopCommandHandler(DiscordSocketClient client, ILogger<StopCommandHandler> logger)
    : SlashCommandHandler(client, logger)
{
    protected override SlashCommandBuilder Command { get; } = new SlashCommandBuilder()
        .WithName("stop")
        .WithDescription((Description)"Remove this channel as one to be counted in.");

    protected override Task HandleAsync(SocketSlashCommand command)
    {
        if (command.Channel.IsDm(command.User))
            return RespondWithError(command, "Channel cannot be DM");

        if (command.Channel.IsThread())
            return RespondWithError(command, "Channel cannot be a thread");

        return RespondWithError(command, "Not implemented yet");
    }
}