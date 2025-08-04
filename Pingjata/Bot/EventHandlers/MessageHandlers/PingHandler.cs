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
        if (message.Channel.IsThread())
            return;

        if (message.Author.IsBot || message.Author.IsWebhook)
            return;

        if (!message.MentionedUserIds.Contains(Client.CurrentUser.Id))
            return;

        await counterService.IncreaseCounter(message.Channel, message.Author.Id);
    }
}