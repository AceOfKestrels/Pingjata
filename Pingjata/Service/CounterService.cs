using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Pingjata.Extensions;
using Pingjata.Persistence;
using Pingjata.Persistence.Models;

namespace Pingjata.Service;

public class CounterService(
    ILogger<CounterService> logger,
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    DiscordSocketClient client)
{
    public async Task<ChannelEntity?> GetChannel(ulong channelId)
    {
        await using ApplicationDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        ChannelEntity? channel = await dbContext.Channels.FindAsync(channelId.ToString());

        return channel;
    }

    /// <summary>
    /// Increases the counter for a given channel if a round is currently active
    /// </summary>
    /// <returns>
    /// -1 if there is currently no round active in this channel, 0 if the counter was increased,
    /// or the channel's threshold if it is reached
    /// </returns>
    public async Task IncreaseCounter(ISocketMessageChannel channel, ulong userId)
    {
        ChannelEntity? entity = await GetChannel(channel.Id);

        if (entity is null || !entity.IsActive)
            return;

        if (entity.CurrentCounter + 1 >= entity.Threshold)
        {
            await FinishRound(channel, userId);

            return;
        }

        await using ApplicationDbContext dbContext = await dbContextFactory.CreateDbContextAsync();
        entity.CurrentCounter++;

        dbContext.Channels.Update(entity);
        await dbContext.SaveChangesAsync();
    }

    public async Task FinishRound(ISocketMessageChannel channel, ulong userId)
    {
        ChannelEntity? entity = await GetChannel(channel.Id);

        if (entity is null || !entity.IsActive)
            return;

        entity.CurrentCounter = 0;
        entity.RoundEndedAt = DateTime.UtcNow;
        entity.WinnerId = userId.ToString();

        SocketUser? user = client.GetUser(userId);

        await using ApplicationDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        dbContext.Channels.Update(entity);
        await dbContext.SaveChangesAsync();

        if (user is not null)
        {
            await channel.SendMessageAsync($"@here\nRound has ended!\nThe winner is {user.Mention}");

            for (int i = 0; i < entity.Threshold; i++)
            {
                await user.SendMessageAsync(user.Mention);
            }

            await user.SendMessageAsync("New threshold please");
        }
    }

    public async Task<int> StartRound(ISocketMessageChannel channel, int min, int? max = null, string? message = null)
    {
        ChannelEntity? entity = await GetChannel(channel.Id);

        entity ??= new() { ChannelId = channel.Id.ToString() };

        await using ApplicationDbContext dbContext = await dbContextFactory.CreateDbContextAsync();

        int value = max is null ? min : Random.Shared.Next(min, max.Value + 1);
        entity.Threshold = value;
        entity.ThresholdRange = max is null ? $"{min}" : $"{min}-{max.Value}";
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
}