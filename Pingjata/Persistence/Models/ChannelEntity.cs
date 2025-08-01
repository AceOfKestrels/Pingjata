// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength
namespace Pingjata.Persistence.Models;

public class ChannelEntity
{
    public string ChannelId { get; set; } = string.Empty;
    public string GreetingMessage { get; set; } = string.Empty;
    public int CurrentCounter { get; set; }
    public int Threshold { get; set; }
    public string ThresholdRange { get; set; } = string.Empty;
    public string? WinnerId { get; set; } = string.Empty;
    public DateTime? RoundEndedAt { get; set; }
    public bool IsPaused { get; set; }

    public bool IsActive => !IsPaused && string.IsNullOrWhiteSpace(WinnerId);
}