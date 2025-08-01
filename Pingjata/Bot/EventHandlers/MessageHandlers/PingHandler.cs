using Discord;
using Discord.WebSocket;
using Pingjata.Bot.EventHandlers.Base;
using Pingjata.Extensions;
using Pingjata.Service;

namespace Pingjata.Bot.EventHandlers.MessageHandlers;

public class PingHandler(DiscordSocketClient client, ILogger<PingHandler> logger, CounterService counterService)
    : MessageEventHandler(client, logger)
{

    protected override async Task HandleAsync(SocketMessage message)
    {
        if(message.Channel.IsThread())
            return;

        if (!message.MentionedUserIds.Contains(Client.CurrentUser.Id))
            return;

        int result = await counterService.IncreaseCounter(message.Channel.Id.ToString());

        logger.LogInformation("Counter increased. Result is {Result}", result);
    }
}