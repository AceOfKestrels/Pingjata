using Microsoft.EntityFrameworkCore;
using Pingjata.Persistence;
using Pingjata.Persistence.Models;
using Pingjata.ResultPattern;

namespace Pingjata.Service;

public class AutoResetService(
    ILogger<AutoResetService> logger,
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    CounterService counterService)
    : IHostedService
{

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = CheckChannels(cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task CheckChannels(CancellationToken cancellationToken)
    {
        await using ApplicationDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            await Task.Delay(TimeSpan.FromSeconds(60), CancellationToken.None);

            List<ChannelEntity> channels = dbContext.Channels
                .Where(c => c.RoundEndedAt - DateTime.UtcNow > TimeSpan.FromMinutes(120))
                .ToList();

            foreach (ChannelEntity channel in channels)
            {
                if (!ulong.TryParse(channel.ChannelId, out ulong channelId))
                    continue;

                Result<int> result = await counterService.RestartRoundForChannel(channelId);

                if (result.IsError)
                    logger.LogWarning(result.Error.LogMessage);
            }
        }
    }
}