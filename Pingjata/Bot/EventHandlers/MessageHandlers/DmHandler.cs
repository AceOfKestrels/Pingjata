using Discord.WebSocket;
using Pingjata.Bot.EventHandlers.Base;
using Pingjata.Extensions;

namespace Pingjata.Bot.EventHandlers.MessageHandlers;

public class DmHandler(DiscordSocketClient client, ILogger<DmHandler> logger) : MessageEventHandler(client, logger)
{
    protected override Task HandleAsync(SocketMessage message)
    {
        if (!message.Channel.IsDm(message.Author))
            return Task.CompletedTask;
        
        logger.LogInformation("DM received: {Content}", message.Content);
        return Task.CompletedTask;
    }
}