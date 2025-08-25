using Discord;
using Discord.WebSocket;
using Pingjata.Bot.EventHandlers.Base;
using Pingjata.Extensions;

namespace Pingjata.Bot.EventHandlers.CommandHandlers;

public class HelpCommandHandler(DiscordSocketClient client, DiscordBot bot, ILogger<HelpCommandHandler> logger)
    : SlashCommandHandler(client, bot, logger)
{
    protected override SlashCommandBuilder Command { get; } = new SlashCommandBuilder()
        .WithName("status")
        .WithDescription((Description)"Show current threshold, count, and state.");

    protected override Task HandleAsync(SocketSlashCommand command)
    {
        if (command.Channel.IsDm(command.User))
            return RespondWithError(command, "Channel cannot be DM");

        if (command.Channel.IsThread())
            return RespondWithError(command, "Channel cannot be a thread");

        return RespondWithError(command, "Not implemented yet");
    }
}