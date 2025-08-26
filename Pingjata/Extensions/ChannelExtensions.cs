using Discord;
using Discord.WebSocket;

namespace Pingjata.Extensions;

public static class ChannelExtensions
{

    private static readonly HashSet<ChannelType> ThreadChannelTypes =
    [
        ChannelType.Forum, ChannelType.News, ChannelType.NewsThread, ChannelType.PrivateThread, ChannelType.PublicThread
    ];

    public static bool IsThread(this ISocketMessageChannel channel)
    {
        return ThreadChannelTypes.Contains(channel.ChannelType);
    }

    public static bool IsDm(this ISocketMessageChannel channel, IUser user)
    {
        return channel.Name == $"@{user.Username}";
    }

    public static string GetChanelLink(this SocketGuildChannel channel)
    {
        return $"https://discord.com/channels/{channel.Guild.Id}/{channel.Id}";
    }
}