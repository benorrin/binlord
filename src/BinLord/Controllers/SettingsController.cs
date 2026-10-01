using BinLord.Data;
using BinLord.Models;
using BinLord.Services;
using Microsoft.AspNetCore.Mvc;

namespace BinLord.Controllers;

public class SettingsController : Controller
{
    private readonly BinLordContext _context;
    private readonly SettingsService _settingsService;
    private readonly NotificationService _notificationService;

    public SettingsController(BinLordContext context, SettingsService settingsService, NotificationService notificationService)
    {
        _context = context;
        _settingsService = settingsService;
        _notificationService = notificationService;
    }

    public async Task<IActionResult> Index()
    {
        var settings = await _settingsService.GetAsync();
        PopulateViewBag(settings);

        // Never send the real password back down to the browser; the view
        // shows a placeholder instead and only changes it if the field is
        // re-entered. This doesn't get saved since GET never calls SaveChanges.
        settings.SmtpPassword = string.Empty;

        return View(settings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(AppSettings model)
    {
        if (!TimeZoneIdIsValid(model.TimeZoneId))
        {
            ModelState.AddModelError(nameof(AppSettings.TimeZoneId), "That time zone isn't recognised by this server.");
        }

        if (model.EmailNotificationsEnabled)
        {
            if (string.IsNullOrWhiteSpace(model.SmtpHost))
            {
                ModelState.AddModelError(nameof(AppSettings.SmtpHost), "SMTP host is required when email notifications are enabled.");
            }

            if (string.IsNullOrWhiteSpace(model.EmailFrom))
            {
                ModelState.AddModelError(nameof(AppSettings.EmailFrom), "From address is required when email notifications are enabled.");
            }

            if (string.IsNullOrWhiteSpace(model.EmailTo))
            {
                ModelState.AddModelError(nameof(AppSettings.EmailTo), "To address is required when email notifications are enabled.");
            }
        }

        if (model.NtfyEnabled && string.IsNullOrWhiteSpace(model.NtfyTopic))
        {
            ModelState.AddModelError(nameof(AppSettings.NtfyTopic), "ntfy topic is required when ntfy notifications are enabled.");
        }

        var settings = await _settingsService.GetAsync();

        if (!ModelState.IsValid)
        {
            PopulateViewBag(settings);
            model.SmtpPassword = string.Empty;
            return View(model);
        }

        settings.AppName = model.AppName.Trim();
        settings.TimeZoneId = model.TimeZoneId;
        settings.BaseUrl = string.IsNullOrWhiteSpace(model.BaseUrl) ? null : model.BaseUrl.Trim().TrimEnd('/');
        settings.BringInAfterHour = model.BringInAfterHour;
        settings.FeedOccurrencesPerBin = model.FeedOccurrencesPerBin;
        settings.HistoryOccurrencesPerBin = model.HistoryOccurrencesPerBin;

        settings.EmailNotificationsEnabled = model.EmailNotificationsEnabled;
        settings.SmtpHost = string.IsNullOrWhiteSpace(model.SmtpHost) ? null : model.SmtpHost.Trim();
        settings.SmtpPort = model.SmtpPort;
        settings.SmtpUseSsl = model.SmtpUseSsl;
        settings.SmtpUsername = string.IsNullOrWhiteSpace(model.SmtpUsername) ? null : model.SmtpUsername.Trim();
        if (!string.IsNullOrEmpty(model.SmtpPassword))
        {
            settings.SmtpPassword = model.SmtpPassword;
        }

        settings.EmailFrom = string.IsNullOrWhiteSpace(model.EmailFrom) ? null : model.EmailFrom.Trim();
        settings.EmailTo = string.IsNullOrWhiteSpace(model.EmailTo) ? null : model.EmailTo.Trim();

        settings.NtfyEnabled = model.NtfyEnabled;
        settings.NtfyServerUrl = model.NtfyServerUrl.Trim().TrimEnd('/');
        settings.NtfyTopic = string.IsNullOrWhiteSpace(model.NtfyTopic) ? null : model.NtfyTopic.Trim();
        settings.NtfyToken = string.IsNullOrWhiteSpace(model.NtfyToken) ? null : model.NtfyToken.Trim();

        await _context.SaveChangesAsync();

        TempData["SettingsSaved"] = "Settings saved.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TestEmail()
    {
        var settings = await _settingsService.GetAsync();
        if (!settings.EmailNotificationsEnabled)
        {
            TempData["SettingsError"] = "Enable and save email notifications before sending a test.";
        }
        else
        {
            var outcome = await _notificationService.SendAsync(settings, "BinLord test notification", "This is a test email from BinLord.");
            TempData[outcome.Email.Success ? "SettingsSaved" : "SettingsError"] = outcome.Email.Success
                ? "Test email sent."
                : $"Test email failed: {outcome.Email.Error}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TestNtfy()
    {
        var settings = await _settingsService.GetAsync();
        if (!settings.NtfyEnabled)
        {
            TempData["SettingsError"] = "Enable and save ntfy notifications before sending a test.";
        }
        else
        {
            var outcome = await _notificationService.SendAsync(settings, "BinLord test notification", "This is a test notification from BinLord.", "bell");
            TempData[outcome.Ntfy.Success ? "SettingsSaved" : "SettingsError"] = outcome.Ntfy.Success
                ? "Test ntfy notification sent."
                : $"Test ntfy notification failed: {outcome.Ntfy.Error}";
        }

        return RedirectToAction(nameof(Index));
    }

    private void PopulateViewBag(AppSettings settings)
    {
        ViewBag.TimeZones = GetTimeZoneSelectItems(settings.TimeZoneId);
        ViewBag.DetectedBaseUrl = $"{Request.Scheme}://{Request.Host}";
        ViewBag.HasSmtpPassword = !string.IsNullOrEmpty(settings.SmtpPassword);
        ViewBag.StatusUrl = settings.GetEffectiveBaseUrl($"{Request.Scheme}://{Request.Host}") + "/api/status";
        ViewBag.HealthUrl = settings.GetEffectiveBaseUrl($"{Request.Scheme}://{Request.Host}") + "/health";
    }

    private static bool TimeZoneIdIsValid(string timeZoneId)
    {
        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return true;
        }
        catch (TimeZoneNotFoundException)
        {
            return false;
        }
        catch (InvalidTimeZoneException)
        {
            return false;
        }
    }

    private static List<(string Id, string DisplayName)> GetTimeZoneSelectItems(string selectedId)
    {
        var zones = TimeZoneInfo.GetSystemTimeZones()
            .OrderBy(z => z.BaseUtcOffset)
            .Select(z => (z.Id, z.DisplayName))
            .ToList();

        if (zones.All(z => z.Id != selectedId))
        {
            zones.Insert(0, (selectedId, $"{selectedId} (not recognised on this server)"));
        }

        return zones;
    }
}
