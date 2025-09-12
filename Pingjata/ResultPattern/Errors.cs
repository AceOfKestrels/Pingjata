namespace Pingjata.ResultPattern;

public static class Errors
{
    private static readonly Error InternalError = new("Error while executing command:",
        "An internal error occurred while executing this command.");

    public static readonly Error CommandInDm = new("", "Channel cannot be DM");
    public static readonly Error CommandInThread = new("", "Channel cannot be a thread");
    public static readonly Error MissingThresholdArg = new("Missing threshold argument");
    public static readonly Error ChannelNotFound = new("No active round in channel");
    public static readonly Error InvalidThreshold = new("Invalid threshold");
    public static readonly Error InvalidChannelId = new("Invalid channel id", InternalError.ResponseMessage);
    public static readonly Error ErrorDmNotWinner = new("User is not a winner");
    public static readonly Error AlreadyPaused = new("Round is already paused");
    public static readonly Error NotPaused = new("Round is not paused");

    public static Error CommandInternalError(Exception ex) => InternalError with { Exception = ex };
    public static Error DatabaseError(Exception ex) => new("Error while accessing database:",
        "An internal error occurred while executing this command.", ex);
}