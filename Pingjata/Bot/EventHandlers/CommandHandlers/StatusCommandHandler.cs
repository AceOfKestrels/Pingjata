using Discord;
using Discord.WebSocket;
using Pingjata.Bot.EventHandlers.Base;
using Pingjata.Extensions;
using Pingjata.Persistence.Models;
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
        .WithDescription((Description)"Show current threshold, count, and state.");

    private const string FailTemplate = "Status: No active round";

    private const string DefaultUserTemplate = "Status: {0}";

    private const string FinishedUserTemplate = """
                                                Status: Finished
                                                Winner: {0}
                                                Round ended {1}
                                                """;

    private const string DefaultAdminTemplate = """
                                                Status: {0}
                                                Counter: {1} / {2}
                                                Greeting: {3}
                                                """;

    private const string FinishedAdminTemplate = """
                                                 Status: Finished
                                                 Counter: {0} / {1}
                                                 Greeting: {2}
                                                 Winner: {3}
                                                 Round ended {4}
                                                 """;

    protected override async Task HandleAsync(SocketSlashCommand command)
    {
        if (command.GuildId is null)
        {
            await RespondWithError(command, "Channel cannot be DM");

            return;
        }

        if (command.Channel.IsThread())
        {
            await RespondWithError(command, "Channel cannot be a thread");

            return;
        }

        ChannelEntity? channel = await counterService.GetChannel(command.Channel.Id);

        string response = await GetResponseMessage(channel, command.User.GetPermissions(command.Channel));

        await command.RespondAsync(response, ephemeral: true);
    }

    private async Task<string> GetResponseMessage(ChannelEntity? channel, ChannelPermissions permissions)
    {
        if (channel is null)
            return FailTemplate;

        string status = channel.IsPaused ? "Paused" : "Active";

        string? userName = !ulong.TryParse(channel.WinnerId, out ulong userId)
            ? null
            : (await Client.GetUserAsync(userId)).Mention;

        if (permissions is { ManageChannel: false, ManageMessages: false })
        {
            if (channel.WinnerId is null || channel.RoundEndedAt is null || userName is null)
                return string.Format(DefaultUserTemplate, status);

            return string.Format(FinishedUserTemplate, userName, channel.RoundEndedAt.Value.ToRelativeTimestamp());
        }

        if (channel.WinnerId is null || channel.RoundEndedAt is null || userName is null)
            return string.Format(DefaultAdminTemplate, status, channel.CurrentCounter, channel.Threshold,
                channel.GreetingMessage);

        return string.Format(FinishedAdminTemplate, channel.Threshold, channel.Threshold, channel.GreetingMessage,
            userName, channel.RoundEndedAt.Value.ToRelativeTimestamp());
    }
}