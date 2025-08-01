using Discord;
using Discord.WebSocket;
using Pingjata.Bot.EventHandlers.Base;

namespace Pingjata.Bot.EventHandlers.CommandHandlers;

public class PingjataCommandHandler(DiscordSocketClient client, ILogger<PingjataCommandHandler> logger) : SlashCommandHandler(client, logger)
{
    private const string StartCommandName = "start";
    private const string PauseCommandName = "pause";
    private const string SetCommandName = "set";
    private const string StopCommandName = "stop";
    private const string StatusCommandName = "status";
    private const string ResetCommandName = "reset";
    private const string HelpCommandName = "help";

    #region CommandBuilder
    protected override SlashCommandBuilder Command { get; } = new SlashCommandBuilder()
        .WithName("pingjata")
        .WithDescription("Manage the Pingjata bot".Take(DescriptionMaxLength).ToString())
        .AddOption(new SlashCommandOptionBuilder()
            .WithType(ApplicationCommandOptionType.SubCommand)
            .WithName(SetCommandName)
            .WithDescription((Description)"Set the threshold for the current channel, will end current round without a winner if used during a round.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("number")
                .WithDescription((Description)"The threshold")
                .WithType(ApplicationCommandOptionType.Integer)
                .WithMinValue(1)
                .WithMaxValue(1000)
                .WithRequired(true)))
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
        logger.LogInformation("Command executed");
        return Task.CompletedTask;
    }
}