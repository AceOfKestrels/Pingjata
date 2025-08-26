using Discord;
using Discord.WebSocket;
using Pingjata.Bot.EventHandlers.Base;
using Pingjata.Extensions;
using Pingjata.ResultPattern;
using Pingjata.Service;

namespace Pingjata.Bot.EventHandlers.CommandHandlers;

public class UnpauseCommandHandler(
    DiscordSocketClient client,
    DiscordBot bot,
    ILogger<UnpauseCommandHandler> logger,
    CounterService counterService)
    : SlashCommandHandler(client, bot, logger)
{
    protected override SlashCommandBuilder Command { get; } = new SlashCommandBuilder()
        .WithName("unpause")
        .WithDescription((Description)"Resume counting.");

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

        Result result = await counterService.PauseRound(command.Channel.Id, false);

        if (result.IsError)
        {
            await RespondWithError(command, result.Error.LogMessage);
        }

        await command.RespondAsync("Successfully unpaused round", ephemeral: true);
    }
}