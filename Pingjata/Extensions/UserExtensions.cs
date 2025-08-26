using Discord;
using Discord.WebSocket;

namespace Pingjata.Extensions;

public static class UserExtensions
{
    public static ChannelPermissions GetPermissions(this IUser user, IChannel channel)
    {
        if (user is not SocketGuildUser guildUser || channel is not SocketGuildChannel guildChannel)
            return default;

        return guildUser.GetPermissions(guildChannel);
    }
}