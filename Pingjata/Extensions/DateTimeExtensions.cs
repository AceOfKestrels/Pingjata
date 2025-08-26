namespace Pingjata.Extensions;

public static class DateTimeExtensions
{
    public static string ToRelativeTimestamp(this DateTime dateTime)
    {
        return $"<t:{dateTime.ToUnixTimeSeconds()}:R>";
    }

    public static long ToUnixTimeSeconds(this DateTime dateTime)
    {
        return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds() / 1000;
    }
}