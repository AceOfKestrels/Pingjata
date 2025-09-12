using Discord;
using Discord.WebSocket;
using Pingjata.Bot.EventHandlers.Base;
using Pingjata.Extensions;
using Pingjata.ResultPattern;
using Pingjata.Service;

namespace Pingjata.Bot.EventHandlers.CommandHandlers;

public class ResetCommandHandler(
    DiscordSocketClient client,
    DiscordBot bot,
    ILogger<ResetCommandHandler> logger,
    CounterService counterService)
    : SlashCommandHandler(client, bot, logger)
{
    protected override SlashCommandBuilder Command { get; } = new SlashCommandBuilder()
        .WithName("reset")
        .WithDescription((Description)Responses.ResetCommandDesc)
        .WithDefaultMemberPermissions(GuildPermission.Administrator | GuildPermission.ManageMessages | GuildPermission.ManageChannels);

    protected override async Task HandleAsync(SocketSlashCommand command)
    {
        Result<int> result = await counterService.RestartRoundForChannel(command.Channel.Id);

        if (result.IsError)
        {
            await RespondWithError(command, result.Error);

            return;
        }

        await command.RespondAsync(Responses.StartSuccess(result.Value), ephemeral: true);
    }
}