using Discord;
using Discord.WebSocket;
using Pingjata.Bot.EventHandlers.Base;
using Pingjata.Extensions;
using Pingjata.Service;

namespace Pingjata.Bot.EventHandlers.CommandHandlers;

public class StartCommandHandler(
    DiscordSocketClient client,
    ILogger<StartCommandHandler> logger,
    CounterService counterService) : SlashCommandHandler(client, logger)
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
            .WithMaxLength(500));

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

        bool isThreshold = int.TryParse(threshold, out int value);
        int result;

        if (isThreshold)
        {
            result = await counterService.StartRound(command.Channel, value, message: message);

            await command.RespondAsync($"Started new round with threshold: {result}", ephemeral: true);

            return;
        }

        string[] splitArg = threshold.Split('-');

        if (splitArg.Length != 2 || !int.TryParse(splitArg[0], out int min) || !int.TryParse(splitArg[1], out int max))
        {
            await RespondWithError(command, "Could not parse threshold or range");

            return;
        }

        result = await counterService.StartRound(command.Channel, min, max, message);
        await command.RespondAsync($"Started new round with threshold: {result}", ephemeral: true);
    }
}