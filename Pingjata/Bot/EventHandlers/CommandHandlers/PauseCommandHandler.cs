using Discord;
using Discord.WebSocket;
using Pingjata.Bot.EventHandlers.Base;
using Pingjata.Extensions;

namespace Pingjata.Bot.EventHandlers.CommandHandlers;

public class PauseCommandHandler(DiscordSocketClient client, DiscordBot bot, ILogger<PauseCommandHandler> logger)
    : SlashCommandHandler(client, bot, logger)
{
    protected override SlashCommandBuilder Command { get; } = new SlashCommandBuilder()
        .WithName("pause")
        .WithDescription((Description)"Pause counting (no new pings recorded).");

    protected override Task HandleAsync(SocketSlashCommand command)
    {
        if (command.Channel.IsDm(command.User))
            return RespondWithError(command, "Channel cannot be DM");

        if (command.Channel.IsThread())
            return RespondWithError(command, "Channel cannot be a thread");

        return RespondWithError(command, "Not implemented yet");
    }
}