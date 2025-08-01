using Discord.WebSocket;

namespace Pingjata.Bot.EventHandlers.Base;

public abstract class MessageEventHandler(DiscordSocketClient client, ILogger<MessageEventHandler> logger) : EventHandler(client)
{
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        Client.MessageReceived += HandleInternalAsync;
        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        Client.MessageReceived -= HandleInternalAsync;
        return Task.CompletedTask;
    }

    private Task HandleInternalAsync(SocketMessage message)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await HandleAsync(message);
            }
            catch (Exception e)
            {
                logger.LogError("Error while handling {Type} event: {Error}", nameof(SocketMessage), e);
            }
        });
        return Task.CompletedTask;
    }

    protected abstract Task HandleAsync(SocketMessage message);
}