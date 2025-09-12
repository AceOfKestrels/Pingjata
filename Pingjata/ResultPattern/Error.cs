namespace Pingjata.ResultPattern;

public record Error(string LogMessage, string ResponseMessage, Exception? Exception = null)
{
    public Error(string message, Exception? exception = null) : this(message, message, exception)
    {
    }

    public static implicit operator Error(string message) => new(message);
}