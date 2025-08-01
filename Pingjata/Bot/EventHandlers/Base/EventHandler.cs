using Discord.WebSocket;

namespace Pingjata.Bot.EventHandlers.Base;

public abstract class EventHandler(DiscordSocketClient client) : IHostedService
{
    protected DiscordSocketClient Client => client;

    protected abstract void Init();
    protected abstract void Destroy();
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        Init();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Destroy();
        return Task.CompletedTask;
    }
}