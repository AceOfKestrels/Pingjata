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

    protected override async Task HandleAsync(SocketSlashCommand command)
    {
        var result = await counterService.StopRound(command.Channel.Id);

        if (result.IsError)
        {
            await RespondWithError(command, result.Error);

            return;
        }

        await command.RespondAsync("Successfully stopped");
    }
}