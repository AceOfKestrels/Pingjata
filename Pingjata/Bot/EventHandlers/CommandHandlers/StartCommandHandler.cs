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
            (Description)Responses.StartCommandDesc)
        .AddOption(new SlashCommandOptionBuilder()
            .WithName(ThresholdOption)
            .WithDescription((Description)Responses.StartCommandThresholdDesc)
            .WithType(ApplicationCommandOptionType.String)
            .WithRequired(true)
            .WithMinLength(1)
            .WithMaxLength(100))
        .AddOption(new SlashCommandOptionBuilder()
            .WithName(MessageOption)
            .WithDescription((Description)Responses.StartCommandMessageDesc)
            .WithType(ApplicationCommandOptionType.String)
            .WithMinLength(1)
            .WithMaxLength(500))
        .WithDefaultMemberPermissions(GuildPermission.Administrator | GuildPermission.ManageMessages | GuildPermission.ManageChannels);

    protected override async Task HandleAsync(SocketSlashCommand command)
    {
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
            await RespondWithError(command, result.Error);

            return;
        }

        await command.RespondAsync(Responses.StartSuccess(result.Value), ephemeral: true);
    }
}