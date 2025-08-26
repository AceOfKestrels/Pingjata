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
        .WithDescription((Description)"Show current threshold, count, and state.")
        .WithDefaultMemberPermissions(GuildPermission.Administrator | GuildPermission.ManageMessages | GuildPermission.ManageChannels);

    protected override async Task HandleAsync(SocketSlashCommand command)
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

        Result<int> result = await counterService.RestartRoundForChannel(command.Channel.Id);

        if (result.IsError)
        {
            await RespondWithError(command, "No round in channel. Use /start instead");

            return;
        }

        await command.RespondAsync($"Started new round with threshold: {result.Value}", ephemeral: true);
    }
}