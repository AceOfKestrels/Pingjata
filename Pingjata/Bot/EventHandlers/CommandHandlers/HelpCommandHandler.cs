using Discord;
using Discord.WebSocket;
using Pingjata.Bot.EventHandlers.Base;
using Pingjata.Extensions;

namespace Pingjata.Bot.EventHandlers.CommandHandlers;

public class HelpCommandHandler(DiscordSocketClient client, DiscordBot bot, ILogger<HelpCommandHandler> logger)
    : SlashCommandHandler(client, bot, logger)
{
    private const string HelpMessage = """
                                       /start <number|low-high> [<message>] — Set this channel as one to be counted in, and set the threshold and optionally a greeting message. Will end current round without a winner if used during a round.
                                       Number: integer
                                       Range: low-high, integers i–z, low ≤ high (inclusive selection).

                                       /pause — Pause counting (no new pings recorded).

                                       /unpause — Resume counting

                                       /stop — Remove this channel as one to be counted in.

                                       /status — Show current threshold, count, and state. 

                                       /reset — Reset the current round (count → 0; keep threshold).

                                       /help — Show usage.
                                       """;

    protected override SlashCommandBuilder Command { get; } = new SlashCommandBuilder()
        .WithName("help")
        .WithDescription((Description)"Show current threshold, count, and state.")
        .WithDefaultMemberPermissions(GuildPermission.Administrator | GuildPermission.ManageMessages | GuildPermission.ManageChannels);

    protected override Task HandleAsync(SocketSlashCommand command)
    {
        if (command.Channel.IsDm(command.User))
            return RespondWithError(command, "Channel cannot be DM");

        if (command.Channel.IsThread())
            return RespondWithError(command, "Channel cannot be a thread");

        return command.RespondAsync(HelpMessage, ephemeral: true);
    }
}