using System.Text.RegularExpressions;
using Discord;
using Discord.WebSocket;
using Pingjata.Bot.EventHandlers.Base;
using Pingjata.Extensions;
using Pingjata.ResultPattern;
using Pingjata.Service;

namespace Pingjata.Bot.EventHandlers.MessageHandlers;

public class DmHandler(DiscordSocketClient client, ILogger<DmHandler> logger, CounterService counterService)
    : MessageEventHandler(client, logger)
{
    private const string Pattern = @".*?(\d(?:-\d)?).*";

    protected override async Task HandleAsync(SocketMessage message)
    {
        if (!message.Channel.IsDm(message.Author))
            return;

        Match match = Regex.Match(message.Content, Pattern, RegexOptions.Compiled);

        if (!match.Success || match.Groups.Count < 2)
            return;

        string threshold = match.Groups[1].Value;

        Result<(int threshold, ulong channelId)> result =
            await counterService.RestartRoundForUser(message.Author.Id, threshold);

        if (result.IsError)
            return;

        await message.Channel.SendMessageAsync(
            $"{result.Value.channelId}\nStarted new round with threshold: {result.Value.threshold}");
    }
}