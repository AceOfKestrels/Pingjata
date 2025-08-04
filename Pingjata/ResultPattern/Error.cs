namespace Pingjata.ResultPattern;

public class Error(string logMessage, string responseMessage)
{
    public string ResponseMessage { get; private set; } = responseMessage;

    public string LogMessage { get; private set; } = logMessage;

    public Error(string message) : this(message, message)
    {
    }

    public static implicit operator Error(string message) => new(message);
}