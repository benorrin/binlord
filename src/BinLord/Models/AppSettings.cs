using System.ComponentModel.DataAnnotations;

namespace BinLord.Models;

/// <summary>
/// Singleton row of app-wide settings. There is always exactly one of
/// these; <see cref="Services.SettingsService"/> creates it on first use.
/// </summary>
public class AppSettings
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "App name")]
    public string AppName { get; set; } = "BinLord";

    [Required]
    [Display(Name = "Time zone")]
    public string TimeZoneId { get; set; } = TimeZoneInfo.Local.Id;

    [StringLength(200)]
    [Url(ErrorMessage = "Enter a full URL, e.g. https://bins.example.com")]
    [Display(Name = "Base URL override")]
    public string? BaseUrl { get; set; }

    [Range(0, 23)]
    [Display(Name = "Prompt to bring bins in after")]
    public int BringInAfterHour { get; set; } = 14;

    [Range(1, 52)]
    [Display(Name = "Occurrences per bin in the RSS feed")]
    public int FeedOccurrencesPerBin { get; set; } = 6;

    [Range(1, 200)]
    [Display(Name = "Occurrences per bin on the History page")]
    public int HistoryOccurrencesPerBin { get; set; } = 12;

    /// <summary>
    /// The base URL to use for absolute links (RSS/calendar feeds): the
    /// configured override if set, otherwise whatever was detected from
    /// the incoming request (which reflects reverse-proxy headers once
    /// forwarded-headers processing has run).
    /// </summary>
    public string GetEffectiveBaseUrl(string detectedBaseUrl)
    {
        return string.IsNullOrWhiteSpace(BaseUrl) ? detectedBaseUrl : BaseUrl.TrimEnd('/');
    }
}
