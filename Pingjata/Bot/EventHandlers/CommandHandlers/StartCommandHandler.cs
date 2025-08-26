using Discord;
using Discord.WebSocket;
using Pingjata.Bot.EventHandlers.Base;
using Pingjata.Extensions;
using Pingjata.ResultPattern;
using Pingjata.Service;

namespace Pingjata.Bot.EventHandlers.CommandHandlers;

public class StartCommandHandler(
    DiscordSocketClient client,
    DiscordBot bot,
    ILogger<StartCommandHandler> logger,
    CounterService counterService) : SlashCommandHandler(client, bot, logger)
{
    private const string ThresholdOption = "threshold";
    private const string MessageOption = "message";

    protected override SlashCommandBuilder Command { get; } = new SlashCommandBuilder()
        .WithName("start")
        .WithDescription(
            (Description)
            "Set this channel as one to be counted in, and set the threshold and optionally a greeting message. Will end current round without a winner if used during a round.")
        .AddOption(new SlashCommandOptionBuilder()
            .WithName(ThresholdOption)
            .WithDescription((Description)"The threshold value or min-max")
            .WithType(ApplicationCommandOptionType.String)
            .WithRequired(true)
            .WithMinLength(1)
            .WithMaxLength(100))
        .AddOption(new SlashCommandOptionBuilder()
            .WithName(MessageOption)
            .WithDescription((Description)"The greeting message to send")
            .WithType(ApplicationCommandOptionType.String)
            .WithMinLength(1)
            .WithMaxLength(500))
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

        string? threshold = command.Data.GetString(ThresholdOption);
        string? message = command.Data.GetString(MessageOption);

        if (threshold is null)
        {
            await RespondWithError(command, "Missing threshold argument");

            return;
        }

        Result<int> result = await counterService.StartRound(command.Channel, threshold, message);

        if (result.IsError)
        {
            await RespondWithError(command, "Could not parse threshold or range");

            return;
        }

        await command.RespondAsync($"Started new round with threshold: {result.Value}", ephemeral: true);
    }
}