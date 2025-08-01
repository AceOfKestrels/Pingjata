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

        return channel.CurrentCounter < channel.Threshold ? 0 : channel.Threshold;
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
}