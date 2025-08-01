using Discord.WebSocket;

namespace Pingjata.Bot.EventHandlers.Base;

public abstract class EventHandler(DiscordSocketClient client) : IHostedService
{
    protected DiscordSocketClient Client => client;

    public abstract Task StartAsync(CancellationToken cancellationToken);

    public abstract Task StopAsync(CancellationToken cancellationToken);
}