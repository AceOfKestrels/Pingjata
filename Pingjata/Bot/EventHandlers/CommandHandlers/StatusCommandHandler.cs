using Discord;
using Discord.WebSocket;
using Pingjata.Bot.EventHandlers.Base;
using Pingjata.Extensions;
using Pingjata.Persistence.Models;
using Pingjata.ResultPattern;
using Pingjata.Service;

namespace Pingjata.Bot.EventHandlers.CommandHandlers;

public class StatusCommandHandler(
    DiscordSocketClient client,
    DiscordBot bot,
    ILogger<StatusCommandHandler> logger,
    CounterService counterService)
    : SlashCommandHandler(client, bot, logger)
{
    protected override SlashCommandBuilder Command { get; } = new SlashCommandBuilder()
        .WithName("status")
        .WithDescription((Description)Responses.StatusCommandDesc);

    protected override async Task HandleAsync(SocketSlashCommand command)
    {
        ChannelEntity? channel = await counterService.GetChannel(command.Channel.Id);

        string response = await GetResponseMessage(channel, command.User.GetPermissions(command.Channel));

        await command.RespondAsync(response, ephemeral: true);
    }

    private async Task<string> GetResponseMessage(ChannelEntity? channel, ChannelPermissions permissions)
    {
        if (channel is null)
            return Responses.StatusNoActiveRound;

        string status = channel.IsPaused ? "Paused" : "Active";

        string? userName = !ulong.TryParse(channel.WinnerId, out ulong userId)
            ? null
            : (await Client.GetUserAsync(userId)).Mention;

        if (permissions is { ManageChannel: false, ManageMessages: false })
        {
            if (channel.WinnerId is null || channel.RoundEndedAt is null || userName is null)
                return Responses.StatusUserActive(channel.IsPaused);

            return Responses.StatusUserFinished(userName, channel.RoundEndedAt.Value);
        }

        if (channel.WinnerId is null || channel.RoundEndedAt is null || userName is null)
            return Responses.StatusAdminActive(channel.IsPaused, channel.CurrentCounter, channel.Threshold,
                channel.GreetingMessage);

        return Responses.StatusAdminFinished(channel.Threshold, channel.Threshold, channel.GreetingMessage,
            userName, channel.RoundEndedAt.Value);
    }
}