using Discord;
using Discord.WebSocket;
using Pingjata.Bot.EventHandlers.Base;
using Pingjata.Extensions;
using Pingjata.ResultPattern;

namespace Pingjata.Bot.EventHandlers.CommandHandlers;

public class HelpCommandHandler(DiscordSocketClient client, DiscordBot bot, ILogger<HelpCommandHandler> logger)
    : SlashCommandHandler(client, bot, logger)
{
    protected override SlashCommandBuilder Command { get; } = new SlashCommandBuilder()
        .WithName("help")
        .WithDescription((Description)Responses.HelpCommandDesc)
        .WithDefaultMemberPermissions(GuildPermission.Administrator | GuildPermission.ManageMessages | GuildPermission.ManageChannels);

    protected override Task HandleAsync(SocketSlashCommand command)
    {
        return command.RespondAsync(Responses.HelpMessage, ephemeral: true);
    }
}