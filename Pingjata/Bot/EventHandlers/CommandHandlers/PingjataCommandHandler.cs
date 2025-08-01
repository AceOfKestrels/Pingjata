using Discord;
using Discord.WebSocket;
using Pingjata.Bot.EventHandlers.Base;

namespace Pingjata.Bot.EventHandlers.CommandHandlers;

public class PingjataCommandHandler(DiscordSocketClient client, ILogger<PingjataCommandHandler> logger)
    : SlashCommandHandler(client, logger)
{
    private const string SetCommandName = "set";
    private const string StartCommandName = "start";
    private const string PauseCommandName = "pause";
    private const string StopCommandName = "stop";
    private const string StatusCommandName = "status";
    private const string ResetCommandName = "reset";
    private const string HelpCommandName = "help";

    private const string SetCommandNumberOptionName = "number";
    private const string SetCommandMinMaxOptionName = "min-max";
    private const string SetCommandMinOptionName = "min";
    private const string SetCommandMaxOptionName = "max";

    #region CommandBuilder

    protected override SlashCommandBuilder Command { get; } = new SlashCommandBuilder()
        .WithName("pingjata")
        .WithDescription("Manage the Pingjata bot".Take(DescriptionMaxLength).ToString())
        .AddOption(new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommand)
            .WithName(SetCommandName)
            .WithDescription(
                (Description)
                "Set the threshold for the current channel, will end current round without a winner if used during a round.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName(SetCommandNumberOptionName)
                .WithDescription((Description)"The threshold")
                .WithType(ApplicationCommandOptionType.Integer)
                .WithMinValue(1)
                .WithMaxValue(1000))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName(SetCommandMinOptionName)
                .WithDescription((Description)"Min")
                .WithType(ApplicationCommandOptionType.Integer)
                .WithMinValue(1)
                .WithMaxValue(1000))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName(SetCommandMaxOptionName)
                .WithDescription((Description)"Max")
                .WithType(ApplicationCommandOptionType.Integer)
                .WithMinValue(1)
                .WithMaxValue(1000)))
        .AddOption(new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommand)
            .WithName(StartCommandName)
            .WithDescription((Description)"Set this channel as one to be counted in, and set a greeting message.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("message")
                .WithDescription((Description)"The greeting message to send")
                .WithType(ApplicationCommandOptionType.String)
                .WithMinLength(1)
                .WithMaxLength(500)
                .WithRequired(true)))
        .AddOption(new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommand)
            .WithName(PauseCommandName)
            .WithDescription((Description)"Pause counting (no new pings recorded)."))
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
        SocketSlashCommandDataOption? subCommand = command.Data.Options.FirstOrDefault();

        if (subCommand is null)
            return RespondWithError(command, "No sub command specified");

        IReadOnlyCollection<SocketSlashCommandDataOption>? args = subCommand.Options;

        return subCommand.Name switch
        {
            SetCommandName => OnSetCommand(command, args.ToList()),
            StartCommandName => OnStartCommand(command, args.ToList()),
            PauseCommandName => OnPauseCommand(command),
            StopCommandName => OnStopCommand(command),
            StatusCommandName => OnStatusCommand(command),
            ResetCommandName => OnResetCommand(command),
            HelpCommandName => OnHelpCommand(command),
            _ => RespondWithError(command, "Unknown subcommand")
        };
    }

    private async Task OnSetCommand(SocketSlashCommand command, List<SocketSlashCommandDataOption>? args)
    {
        if (args is null || args.Count == 0)
        {
            await RespondWithError(command, "No arguments");

            return;
        }

        SocketSlashCommandDataOption firstArg = args[0];
        bool isThreshold = firstArg.Name == SetCommandNumberOptionName;

        if (isThreshold)
        {
            await command.RespondAsync($"Set command executed with threshold: {firstArg.Value}", ephemeral: true);

            return;
        }

        if (firstArg.Name != SetCommandMinOptionName && firstArg.Name != SetCommandMaxOptionName)
        {
            await RespondWithError(command, "Unknown argument at index 0");

            return;
        }

        // TODO: check second arg and min/max
    }

    private async Task OnStartCommand(SocketSlashCommand command, List<SocketSlashCommandDataOption>? args)
    {
        await command.RespondAsync("Start command executed", ephemeral: true);
    }

    private async Task OnPauseCommand(SocketSlashCommand command)
    {
        await command.RespondAsync("Pause command executed", ephemeral: true);
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
        logger.LogError("Received invalid command: {Error}", error);

        return command.RespondAsync(error, ephemeral: true);
    }
}