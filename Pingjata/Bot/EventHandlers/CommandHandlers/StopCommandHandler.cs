using Discord;
using Discord.WebSocket;
using Pingjata.Bot.EventHandlers.Base;
using Pingjata.Extensions;
using Pingjata.Service;

namespace Pingjata.Bot.EventHandlers.CommandHandlers;

public class StopCommandHandler(
    DiscordSocketClient client,
    DiscordBot bot,
    CounterService counterService,
    ILogger<StopCommandHandler> logger)
    : SlashCommandHandler(client, bot, logger)
{
    protected override SlashCommandBuilder Command { get; } = new SlashCommandBuilder()
        .WithName("stop")
        .WithDescription((Description)"Remove this channel as one to be counted in.")
        .WithDefaultMemberPermissions(GuildPermission.Administrator | GuildPermission.ManageMessages |
                                      GuildPermission.ManageChannels);

    protected async override Task HandleAsync(SocketSlashCommand command)
    {
        if (command.Channel.IsDm(command.User))
        {
            await RespondWithError(command, "Channel cannot be DM");

            return;
        }

        if (command.Channel.IsThread())
        {
            await RespondWithError(command, "Channel cannot be a thread");

            return;
        }

        var result = await counterService.StopRound(command.Channel.Id);

        if (result.IsError)
        {
            await RespondWithError(command, result.Error.LogMessage);

            return;
        }

        await command.RespondAsync("Successfully stopped");
    }
}