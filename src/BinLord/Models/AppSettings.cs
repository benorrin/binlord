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

    [Display(Name = "Enable email notifications")]
    public bool EmailNotificationsEnabled { get; set; }

    [StringLength(200)]
    [Display(Name = "SMTP host")]
    public string? SmtpHost { get; set; }

    [Range(1, 65535)]
    [Display(Name = "SMTP port")]
    public int SmtpPort { get; set; } = 587;

    [Display(Name = "Use TLS")]
    public bool SmtpUseSsl { get; set; } = true;

    [StringLength(200)]
    [Display(Name = "SMTP username")]
    public string? SmtpUsername { get; set; }

    [StringLength(500)]
    [Display(Name = "SMTP password")]
    public string? SmtpPassword { get; set; }

    [StringLength(200)]
    [EmailAddress]
    [Display(Name = "From address")]
    public string? EmailFrom { get; set; }

    [StringLength(200)]
    [EmailAddress]
    [Display(Name = "To address")]
    public string? EmailTo { get; set; }

    [Display(Name = "Enable ntfy notifications")]
    public bool NtfyEnabled { get; set; }

    [Required]
    [StringLength(200)]
    [Url(ErrorMessage = "Enter a full URL, e.g. https://ntfy.sh")]
    [Display(Name = "ntfy server URL")]
    public string NtfyServerUrl { get; set; } = "https://ntfy.sh";

    [StringLength(100)]
    [Display(Name = "ntfy topic")]
    public string? NtfyTopic { get; set; }

    [StringLength(200)]
    [Display(Name = "ntfy access token (optional)")]
    public string? NtfyToken { get; set; }

    [Display(Name = "Schedules are publicly visible")]
    public bool SchedulesPubliclyVisible { get; set; } = true;

    /// <summary>
    /// Lets the RSS/calendar feeds stay usable when schedules aren't public:
    /// append <c>?token=...</c> to bypass the login requirement, since feed
    /// readers and calendar apps can't do interactive logins.
    /// </summary>
    [StringLength(100)]
    public string? FeedAccessToken { get; set; }

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

    /// <summary>
    /// Whether an anonymous request should be let through: either schedules
    /// are public, or it supplied a valid feed access token.
    /// </summary>
    public bool AllowsAnonymousFeedAccess(string? suppliedToken)
    {
        if (SchedulesPubliclyVisible)
        {
            return true;
        }

        return !string.IsNullOrEmpty(FeedAccessToken) && suppliedToken == FeedAccessToken;
    }
}
