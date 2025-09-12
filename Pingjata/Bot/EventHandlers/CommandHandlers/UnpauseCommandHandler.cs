using Discord;
using Discord.WebSocket;
using Pingjata.Bot.EventHandlers.Base;
using Pingjata.Extensions;
using Pingjata.ResultPattern;
using Pingjata.Service;

namespace Pingjata.Bot.EventHandlers.CommandHandlers;

public class UnpauseCommandHandler(
    DiscordSocketClient client,
    DiscordBot bot,
    ILogger<UnpauseCommandHandler> logger,
    CounterService counterService)
    : SlashCommandHandler(client, bot, logger)
{
    protected override SlashCommandBuilder Command { get; } = new SlashCommandBuilder()
        .WithName("unpause")
        .WithDescription((Description)Responses.UnpauseCommandDesc)
        .WithDefaultMemberPermissions(GuildPermission.Administrator | GuildPermission.ManageMessages | GuildPermission.ManageChannels);

    protected override async Task HandleAsync(SocketSlashCommand command)
    {
        Result result = await counterService.PauseRound(command.Channel.Id, false);

        if (result.IsError)
        {
            await RespondWithError(command, result.Error);
        }

        await command.RespondAsync(Responses.UnpauseSuccess, ephemeral: true);
    }
}