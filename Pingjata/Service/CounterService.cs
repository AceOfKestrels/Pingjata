using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Pingjata.Extensions;
using Pingjata.Persistence;
using Pingjata.Persistence.Models;
using Pingjata.ResultPattern;

namespace Pingjata.Service;

public class CounterService(
    ILogger<CounterService> logger,
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    PingService pingService,
    DiscordSocketClient client)
{
    public async Task<Result<ChannelEntity>> GetChannel(ulong channelId)
    {
        await using ApplicationDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        ChannelEntity? channel = await dbContext.Channels.FindAsync(channelId.ToString());

        if (channel is null)
            return (Error)"Channel not found";

        return channel;
    }

    /// <summary>
    /// Increases the counter for a given channel if a round is currently active
    /// </summary>
    public async Task<Result> IncreaseCounter(ISocketMessageChannel channel, ulong userId)
    {
        ChannelEntity? entity = await GetChannel(channel.Id);

        if (entity is null || !entity.IsActive)
            return "No round active in channel";

        if (entity.CurrentCounter + 1 >= entity.Threshold)
        {
            return await FinishRound(channel, userId);
        }

        await using ApplicationDbContext dbContext = await dbContextFactory.CreateDbContextAsync();
        entity.CurrentCounter++;

        dbContext.Channels.Update(entity);
        await dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<Result> FinishRound(ISocketMessageChannel channel, ulong userId)
    {
        ChannelEntity? entity = await GetChannel(channel.Id);

        if (entity is null || !entity.IsActive)
            return "No round active in channel";

        entity.CurrentCounter = 0;
        entity.RoundEndedAt = DateTime.UtcNow;
        entity.WinnerId = userId.ToString();

        SocketUser? user = client.GetUser(userId);

        await using ApplicationDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        dbContext.Channels.Update(entity);
        await dbContext.SaveChangesAsync();

        if (user is not null)
        {
            await channel.SendMessageAsync(
                $"@here\nRound has ended!\nThe winner is {user.Mention}\nTotal pings: {entity.Threshold}");

            await user.SendMessageAsync("New threshold please");

            for (int i = 0; i < entity.Threshold; i++)
            {
                pingService.QueuePing(user);
            }
        }

        return true;
    }

    public async Task<Result<int>> StartRound(ISocketMessageChannel channel, string? threshold = null,
        string? message = null)
    {
        ChannelEntity? entity = await GetChannel(channel.Id);

        entity ??= new() { ChannelId = channel.Id.ToString() };

        await using ApplicationDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        int? value = GetThresholdValue(threshold ?? entity.ThresholdRange);

        if (!value.HasValue)
            return (Error)"Invalid threshold";

        entity.Threshold = value.Value;
        entity.ThresholdRange = threshold ?? entity.ThresholdRange;
        entity.CurrentCounter = 0;
        entity.IsPaused = false;
        entity.WinnerId = null;
        entity.RoundEndedAt = null;
        if (message is not null)
            entity.GreetingMessage = message;

        if (entity.GreetingMessage.IsEmpty())
            entity.GreetingMessage = "Heyo, Pingjata party time!";

        dbContext.Channels.Update(entity);
        await dbContext.SaveChangesAsync();

        await channel.SendMessageAsync(entity.GreetingMessage);

        return value;
    }

    public async Task<Result<int>> RestartRound(ulong channelId, string? threshold = null)
    {
        ISocketMessageChannel? channel = (await client.GetChannelAsync(channelId)) as ISocketMessageChannel;

        if (channel is null)
            return (Error)"Unknown channel";

        return await StartRound(channel, threshold);
    }

    private int? GetThresholdValue(string threshold)
    {
        if (int.TryParse(threshold, out int value))
            return value;

        string[] splitArg = threshold.Split('-');

        if (splitArg.Length != 2 || !int.TryParse(splitArg[0], out int min) || !int.TryParse(splitArg[1], out int max))
            return null;

        return Random.Shared.Next(min, max + 1);
    }
}