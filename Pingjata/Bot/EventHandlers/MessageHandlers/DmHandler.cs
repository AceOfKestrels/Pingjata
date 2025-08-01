using Discord.WebSocket;
using Pingjata.Bot.EventHandlers.Base;

namespace Pingjata.Bot.EventHandlers.MessageHandlers;

public class DmHandler(DiscordSocketClient client, ILogger<DmHandler> logger) : MessageEventHandler(client, logger)
{
    protected override Task HandleAsync(SocketMessage message)
    {
        if (message.Channel.Name != $"@{message.Author.Username}")
            return Task.CompletedTask;
        
        logger.LogInformation("DM received: {Content}", message.Content);
        return Task.CompletedTask;
    }
}