using Discord.WebSocket;
using Pingjata.Bot.EventHandlers.Base;

namespace Pingjata.Bot.EventHandlers.MessageHandlers;

public class PingHandler(DiscordSocketClient client, ILogger<PingHandler> logger) : MessageEventHandler(client, logger)
{
    protected override Task HandleAsync(SocketMessage message)
    {
        if(!message.MentionedUserIds.Contains(Client.CurrentUser.Id))
            return Task.CompletedTask;
        
        logger.LogInformation("Mentioned");
        return Task.CompletedTask;
    }
}