using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Pingjata.Extensions;
using Pingjata.Persistence;
using Pingjata.Persistence.Models;
using Pingjata.ResultPattern;

namespace Pingjata.Service;

public class CounterService(
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    PingService pingService,
    DiscordSocketClient client)
{
    public async Task<Result<ChannelEntity>> GetChannel(ulong channelId)
    {
        try
        {
            await using ApplicationDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

            ChannelEntity? channel = await dbContext.Channels.FindAsync(channelId.ToString());

            if (channel is null)
                return Errors.ChannelNotFound;

            return channel;
        }
        catch (Exception e)
        {
            return Errors.DatabaseError(e);
        }
    }

    /// <summary>
    /// Increases the counter for a given channel if a round is currently active
    /// </summary>
    public async Task<Result> IncreaseCounter(ISocketMessageChannel channel, ulong userId)
    {
        Result<ChannelEntity> result = await GetChannel(channel.Id);

        if (result.IsError || !result.Value.IsActive)
            return result.Error ?? Errors.ChannelNotFound;

        ChannelEntity entity = result.Value;

        if (entity.CurrentCounter + 1 >= entity.Threshold)
        {
            return await FinishRound(channel, userId);
        }

        await using ApplicationDbContext dbContext = await dbContextFactory.CreateDbContextAsync();
        entity.CurrentCounter++;

        dbContext.Channels.Update(entity);

        return await dbContext.TrySaveChangesAsync();
    }

    public async Task<Result> FinishRound(ISocketMessageChannel channel, ulong userId)
    {
        Result<ChannelEntity> result = await GetChannel(channel.Id);

        if (result.IsError || !result.Value.IsActive)
            return result.Error ?? Errors.ChannelNotFound;

        ChannelEntity entity = result.Value;

        entity.CurrentCounter = 0;
        entity.RoundEndedAt = DateTime.UtcNow;
        entity.WinnerId = userId.ToString();

        SocketUser? user = client.GetUser(userId);

        await using ApplicationDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        dbContext.Channels.Update(entity);

        Result saveResult = await dbContext.TrySaveChangesAsync();

        if (user is not null && !saveResult.IsError)
        {
            await channel.SendMessageAsync(Responses.WinnerAnnouncement(user.Mention, entity.Threshold));

            await user.SendMessageAsync(Responses.RequestNewThreshold);

            for (int i = 0; i < entity.Threshold; i++)
            {
                pingService.QueuePing(user);
            }
        }

        return saveResult;
    }

    public async Task<Result<int>> StartRound(ISocketMessageChannel channel, string? threshold = null,
        string? message = null)
    {
        ChannelEntity? entity = await GetChannel(channel.Id);

        bool newRound = false;

        if (entity is null)
        {
            newRound = true;
            entity ??= new() { ChannelId = channel.Id.ToString() };
        }

        int? value = GetThresholdValue(threshold ?? entity.ThresholdRange);

        if (!value.HasValue)
            return Errors.InvalidThreshold;

        entity.Threshold = value.Value;
        entity.ThresholdRange = threshold ?? entity.ThresholdRange;
        entity.CurrentCounter = 0;
        entity.IsPaused = false;
        entity.WinnerId = null;
        entity.RoundEndedAt = null;
        if (message is not null)
            entity.GreetingMessage = message;

        if (entity.GreetingMessage.IsEmpty())
            entity.GreetingMessage = Responses.DefaultGreeting;

        await using ApplicationDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        if (newRound)
            dbContext.Channels.Add(entity);
        else
            dbContext.Channels.Update(entity);

        Result saved = await dbContext.TrySaveChangesAsync();

        if (saved.IsError)
            return saved.Error;

        await channel.SendMessageAsync(entity.GreetingMessage);

        return value;
    }

    public async Task<Result<int>> RestartRoundForChannel(ulong channelId, string? threshold = null)
    {
        ISocketMessageChannel? channel = (await client.GetChannelAsync(channelId)) as ISocketMessageChannel;

        if (channel is null)
            return Errors.InvalidChannelId;

        return await StartRound(channel, threshold);
    }

    public async Task<Result<(int threshold, ulong channelId)>> RestartRoundForUser(ulong userId, string threshold)
    {
        await using ApplicationDbContext dbContext = await dbContextFactory.CreateDbContextAsync();
        ChannelEntity? entity = await dbContext.Channels.FirstOrDefaultAsync(c => c.WinnerId == userId.ToString());

        if (entity is null)
            return Errors.ErrorDmNotWinner;

        if (!ulong.TryParse(entity.ChannelId, out ulong channelId))
            return Errors.InvalidChannelId;

        Result<int> result = await RestartRoundForChannel(channelId, threshold);

        if (result.IsError)
            return result.Error;

        return (result.Value, channelId);
    }

    public async Task<Result> PauseRound(ulong channelId, bool state)
    {
        Result<ChannelEntity> result = await GetChannel(channelId);

        if (result.IsError || !result.Value.IsActive)
            return result.Error ?? Errors.ChannelNotFound;

        ChannelEntity channel = result.Value;

        if (channel.IsPaused == state)
            return state ? Errors.AlreadyPaused : Errors.NotPaused;

        channel.IsPaused = state;
        await using ApplicationDbContext dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.Channels.Update(channel);

        return await dbContext.TrySaveChangesAsync();
    }

    public async Task<Result> StopRound(ulong channelId)
    {
        Result<ChannelEntity> result = await GetChannel(channelId);

        if (result.IsError || !result.Value.IsActive)
            return result.Error ?? Errors.ChannelNotFound;

        ChannelEntity channel = result.Value;

        await using ApplicationDbContext dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.Channels.Remove(channel);

        return await dbContext.TrySaveChangesAsync();
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