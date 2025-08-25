using System.Globalization;
using Discord;
using Discord.WebSocket;

namespace Pingjata.Extensions;

public static class SlashCommandExtensions
{
    public static string? GetString(this SocketSlashCommandData data, string argName)
    {
        SocketSlashCommandDataOption? option = data.Options.FirstOrDefault(o => o.Name.Equals(argName, StringComparison.OrdinalIgnoreCase));
        return option?.Value as string;
    }

    public static long? GetNumber(this SocketSlashCommandData data, string argName)
    {
        SocketSlashCommandDataOption? option = data.Options.FirstOrDefault(o => o.Name.Equals(argName, StringComparison.OrdinalIgnoreCase));

        if (option is null)
            return null;

        if (option.Type == ApplicationCommandOptionType.Number)
            return (long)option.Value;

        if (!long.TryParse(option.Value.ToString(), out long value))
            return null;

        return value;
    }
}