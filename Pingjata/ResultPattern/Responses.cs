using Pingjata.Extensions;

namespace Pingjata.ResultPattern;

public static class Responses
{
    public const string HelpMessage = """
                                      /start <number|low-high> [<message>] — Set this channel as one to be counted in, and set the threshold and optionally a greeting message. Will end current round without a winner if used during a round.
                                      Number: integer
                                      Range: low-high, integers i–z, low ≤ high (inclusive selection).

                                      /pause — Pause counting (no new pings recorded).

                                      /unpause — Resume counting

                                      /stop — Remove this channel as one to be counted in.

                                      /status — Show current threshold, count, and state. 

                                      /reset — Reset the current round (count → 0; keep threshold).

                                      /help — Show usage.
                                      """;

    public const string HelpCommandDesc = "Display help";
    public const string PauseCommandDesc = "Pause counting (no new pings recorded).";
    public const string ResetCommandDesc = "Show current threshold, count, and state.";

    public const string StartCommandDesc =
        "Set this channel as one to be counted in, and set the threshold and optionally a greeting message. Will end current round without a winner if used during a round.";

    public const string StartCommandThresholdDesc = "The threshold value or min-max";
    public const string StartCommandMessageDesc = "The greeting message to send";

    public const string StatusCommandDesc = "Show current threshold, count, and state.";
    public const string StopCommandDesc = "Remove this channel as one to be counted in.";
    public const string UnpauseCommandDesc = "Resume counting.";

    public const string PauseSuccess = "Successfully paused round";
    public const string UnpauseSuccess = "Successfully unpaused round";
    public const string StopSuccess = "Successfully stopped";
    public const string StatusNoActiveRound = "Status: No active round";
    public const string DefaultGreeting = "Heyo, Pingjata party time!";
    public const string RequestNewThreshold = "New threshold please";

    public static string StatusName(bool paused) => paused ? "Paused" : "Active";

    public static string StartSuccess(int threshold) => $"Started new round with threshold: {threshold}";

    public static string StatusUserActive(bool paused) => $"Status: {StatusName(paused)}";

    public static string StatusUserFinished(string winner, DateTime endedAt) => $"""
         Status: Finished
         Winner: {winner}
         Round ended {endedAt.ToRelativeTimestamp()}
         """;

    public static string StatusAdminActive(bool paused, int currentCount, int threshold, string greeting) => $"""
         Status: {StatusName(paused)}
         Counter: {currentCount} / {threshold}
         Greeting: {greeting}
         """;

    public static string StatusAdminFinished(int currentCount, int threshold, string greeting, string winner,
        DateTime endedAt) => $"""
                              Status: Finished
                              Counter: {currentCount} / {threshold}
                              Greeting: {greeting}
                              Winner: {winner}
                              Round ended {endedAt.ToRelativeTimestamp()}
                              """;

    public static string WinnerAnnouncement(string winner, int threshold) => $"""
                                                                              @here
                                                                              Round has ended!
                                                                              The winner is {winner}
                                                                              Total pings: {threshold}
                                                                              """;
}