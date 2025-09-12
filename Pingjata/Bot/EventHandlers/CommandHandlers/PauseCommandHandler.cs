using Discord;
using Discord.WebSocket;
using Pingjata.Bot.EventHandlers.Base;
using Pingjata.Extensions;
using Pingjata.ResultPattern;
using Pingjata.Service;

namespace Pingjata.Bot.EventHandlers.CommandHandlers;

public class PauseCommandHandler(
    DiscordSocketClient client,
    DiscordBot bot,
    ILogger<PauseCommandHandler> logger,
    CounterService counterService)
    : SlashCommandHandler(client, bot, logger)
{
    protected override SlashCommandBuilder Command { get; } = new SlashCommandBuilder()
        .WithName("pause")
        .WithDescription((Description)"Pause counting (no new pings recorded).")
        .WithDefaultMemberPermissions(GuildPermission.Administrator | GuildPermission.ManageMessages | GuildPermission.ManageChannels);

    protected override async Task HandleAsync(SocketSlashCommand command)
    {
        Result result = await counterService.PauseRound(command.Channel.Id, true);

        if (result.IsError)
        {
            await RespondWithError(command, result.Error);
        }

        await command.RespondAsync("Successfully paused round", ephemeral: true);
    }
}