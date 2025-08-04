using Discord;
using Discord.WebSocket;
using Pingjata.Bot.EventHandlers.Base;
using Pingjata.Extensions;
using Pingjata.Service;

namespace Pingjata.Bot.EventHandlers.CommandHandlers;

public class PingjataCommandHandler(
    DiscordSocketClient client,
    ILogger<PingjataCommandHandler> logger,
    CounterService counterService)
    : SlashCommandHandler(client, logger)
{
    private const string StartCommandName = "start";
    private const string PauseCommandName = "pause";
    private const string UnpauseCommandName = "unpause";
    private const string StopCommandName = "stop";
    private const string StatusCommandName = "status";
    private const string ResetCommandName = "reset";
    private const string HelpCommandName = "help";

    private const string StartCommandNumberOptionName = "value";
    private const string StartCommandMessageOptionName = "message";

    #region CommandBuilder

    protected override SlashCommandBuilder Command { get; } = new SlashCommandBuilder()
        .WithName("pingjata")
        .WithDescription((Description)"Manage the Pingjata bot")
        .AddOption(new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommand)
            .WithName(StartCommandName)
            .WithDescription(
                (Description)
                "Set the threshold for the current channel, will end current round without a winner if used during a round.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName(StartCommandNumberOptionName)
                .WithDescription((Description)"The threshold value or min-max")
                .WithType(ApplicationCommandOptionType.String)
                .WithRequired(true)
                .WithMinLength(1)
                .WithMaxLength(100))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName(StartCommandMessageOptionName)
                .WithDescription((Description)"The greeting message to send")
                .WithType(ApplicationCommandOptionType.String)
                .WithMinLength(1)
                .WithMaxLength(500)))
        .AddOption(new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommand)
            .WithName(PauseCommandName)
            .WithDescription((Description)"Pause counting (no new pings recorded)."))
        .AddOption(new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommand)
            .WithName(UnpauseCommandName)
            .WithDescription((Description)"Resume counting."))
        .AddOption(new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommand)
            .WithName(StopCommandName)
            .WithDescription((Description)"Remove this channel as one to be counted in."))
        .AddOption(new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommand)
            .WithName(StatusCommandName)
            .WithDescription((Description)"Show current threshold, count, and state."))
        .AddOption(new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommand)
            .WithName(ResetCommandName)
            .WithDescription((Description)"Reset the current round (count → 0; keep threshold)."))
        .AddOption(new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommand)
            .WithName(HelpCommandName)
            .WithDescription((Description)"Show usage."));

    #endregion

    protected override Task HandleAsync(SocketSlashCommand command)
    {
        if (command.Channel.IsThread())
        {
            return RespondWithError(command, "Channel cannot be a thread");
        }

        SocketSlashCommandDataOption? subCommand = command.Data.Options.FirstOrDefault();

        if (subCommand is null)
            return RespondWithError(command, "No sub command specified");

        IReadOnlyCollection<SocketSlashCommandDataOption>? args = subCommand.Options;

        return subCommand.Name switch
        {
            StartCommandName => OnStartCommand(command, args.ToList()),
            PauseCommandName => OnPauseCommand(command),
            UnpauseCommandName => OnUnpauseCommand(command),
            StopCommandName => OnStopCommand(command),
            StatusCommandName => OnStatusCommand(command),
            ResetCommandName => OnResetCommand(command),
            HelpCommandName => OnHelpCommand(command),
            _ => RespondWithError(command, "Unknown subcommand")
        };
    }

    private async Task OnStartCommand(SocketSlashCommand command, List<SocketSlashCommandDataOption>? args)
    {
        if (args is null || args.Count == 0)
        {
            await RespondWithError(command, "No arguments");

            return;
        }

        SocketSlashCommandDataOption? thresholdArg = args.FirstOrDefault(a => a.Name == StartCommandNumberOptionName);

        if (thresholdArg is null || thresholdArg.Name != StartCommandNumberOptionName || thresholdArg.Value is not string argStr)
        {
            await RespondWithError(command, "Unknown argument at index 0");

            return;
        }

        string? greetingArg = args.FirstOrDefault(a => a.Name == StartCommandMessageOptionName)?.Value as string;

        bool isThreshold = int.TryParse(argStr, out int value);
        int result;

        if (isThreshold)
        {
            result = await counterService.StartRound(command.Channel, value, message: greetingArg);

            await command.RespondAsync($"Started new round with threshold: {result}", ephemeral: true);

            return;
        }

        string[] splitArg = argStr.Split('-');

        if (splitArg.Length != 2 || !int.TryParse(splitArg[0], out int min) || !int.TryParse(splitArg[1], out int max))
        {
            await RespondWithError(command, "Could not parse threshold or range");

            return;
        }

        result = await counterService.StartRound(command.Channel, min, max, greetingArg);
        await command.RespondAsync($"Started new round with threshold: {result}", ephemeral: true);
    }

    private async Task OnPauseCommand(SocketSlashCommand command)
    {
        await command.RespondAsync("Pause command executed", ephemeral: true);
    }

    private async Task OnUnpauseCommand(SocketSlashCommand command)
    {
        await command.RespondAsync("Unpause command executed", ephemeral: true);
    }

    private async Task OnStopCommand(SocketSlashCommand command)
    {
        await command.RespondAsync("Stop command executed", ephemeral: true);
    }

    private async Task OnStatusCommand(SocketSlashCommand command)
    {
        await command.RespondAsync("Status command executed", ephemeral: true);
    }

    private async Task OnResetCommand(SocketSlashCommand command)
    {
        await command.RespondAsync("Reset command executed", ephemeral: true);
    }

    private async Task OnHelpCommand(SocketSlashCommand command)
    {
        await command.RespondAsync("Help command executed", ephemeral: true);
    }

    private Task RespondWithError(SocketSlashCommand command, string error)
    {
        logger.LogWarning("Received invalid command: {Error}", error);

        return command.RespondAsync(error, ephemeral: true);
    }
}