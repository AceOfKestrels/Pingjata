using Microsoft.EntityFrameworkCore;
using Pingjata.Persistence;
using Pingjata.Persistence.Models;

namespace Pingjata.Service;

public class CounterService(ILogger<CounterService> logger, IDbContextFactory<ApplicationDbContext> dbContextFactory)
{
    public async Task<ChannelEntity?> GetChannel(string channelId)
    {
        await using ApplicationDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        ChannelEntity? channel = await dbContext.Channels.FindAsync(channelId);

        return channel;
    }

    /// <summary>
    /// Increases the counter for a given channel, if a round is currently active
    /// </summary>
    /// <returns>
    /// -1 if there is currently no round active in this channel, 0 if the counter was increased,
    /// or the channel's threshold if it is reached
    /// </returns>
    public async Task<int> IncreaseCounter(string channelId)
    {
        ChannelEntity? channel = await GetChannel(channelId);

        if (channel is null || !channel.IsActive)
            return -1;

        await using ApplicationDbContext dbContext = await dbContextFactory.CreateDbContextAsync();
        channel.CurrentCounter++;

        dbContext.Channels.Update(channel);
        await dbContext.SaveChangesAsync();

        return channel.CurrentCounter < channel.Threshold ? channel.CurrentCounter : 0;
    }

    public async Task<bool> StartRound(string channelId, string message)
    {
        ChannelEntity? channel = await GetChannel(channelId);

        await using ApplicationDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        if (channel is null)
        {
            channel = new()
            {
                ChannelId = channelId,
                GreetingMessage = message,
                IsPaused = true
            };

            dbContext.Channels.Add(channel);
            await dbContext.SaveChangesAsync();
            return false;
        }

        channel.GreetingMessage = message;
        dbContext.Channels.Update(channel);
        await dbContext.SaveChangesAsync();

        return !channel.IsActive;
    }

    public async Task<int> SetThreshold(string channelId, int min, int? max = null)
    {
        ChannelEntity? channel = await GetChannel(channelId);

        if (channel is null)
            return -1;

        await using ApplicationDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        int value = max is null ? min : Random.Shared.Next(min, max.Value + 1);
        channel.Threshold = value;
        channel.ThresholdRange = max is null ? $"{min}" : $"{min}-{max.Value}";
        channel.CurrentCounter = 0;
        channel.IsPaused = false;
        channel.WinnerId = null;
        channel.RoundEndedAt = null;

        dbContext.Channels.Update(channel);
        await dbContext.SaveChangesAsync();

        return value;
    }
}