using Discord;
using Discord.WebSocket;
using Pingjata.Bot.EventHandlers.Base;
using Pingjata.Extensions;

namespace Pingjata.Bot.EventHandlers.CommandHandlers;

public class UnpauseCommandHandler(DiscordSocketClient client, ILogger<UnpauseCommandHandler> logger)
    : SlashCommandHandler(client, logger)
{
    protected override SlashCommandBuilder Command { get; } = new SlashCommandBuilder()
        .WithName("unpause")
        .WithDescription((Description)"Resume counting.");

    protected override Task HandleAsync(SocketSlashCommand command)
    {
        if (command.Channel.IsDm(command.User))
            return RespondWithError(command, "Channel cannot be DM");

        if (command.Channel.IsThread())
            return RespondWithError(command, "Channel cannot be a thread");

        return RespondWithError(command, "Not implemented yet");
    }
}