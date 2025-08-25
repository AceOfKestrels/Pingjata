using Discord;
using Discord.WebSocket;

namespace Pingjata.Service;

public class PingService(DiscordSocketClient client) : IHostedService
{
    private readonly Queue<SocketUser> Queue = [];

    public void QueuePing(SocketUser user)
    {
        Queue.Enqueue(user);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = SendPings(cancellationToken);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task SendPings(CancellationToken cancellationToken)
    {
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            if (Queue.Count == 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), CancellationToken.None);

                continue;
            }

            SocketUser user = Queue.Dequeue();
            await user.SendMessageAsync(user.Mention);
        }
    }
}