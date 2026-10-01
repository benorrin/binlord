using BinLord.Data;
using BinLord.Models;
using BinLord.Services;
using Microsoft.AspNetCore.Mvc;

namespace BinLord.Controllers;

public class SettingsController : Controller
{
    private readonly BinLordContext _context;
    private readonly SettingsService _settingsService;

    public SettingsController(BinLordContext context, SettingsService settingsService)
    {
        _context = context;
        _settingsService = settingsService;
    }

    public async Task<IActionResult> Index()
    {
        var settings = await _settingsService.GetAsync();
        ViewBag.TimeZones = GetTimeZoneSelectItems(settings.TimeZoneId);
        ViewBag.DetectedBaseUrl = $"{Request.Scheme}://{Request.Host}";
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

        if (!ModelState.IsValid)
        {
            ViewBag.TimeZones = GetTimeZoneSelectItems(model.TimeZoneId);
            ViewBag.DetectedBaseUrl = $"{Request.Scheme}://{Request.Host}";
            return View(model);
        }

        var settings = await _settingsService.GetAsync();
        settings.AppName = model.AppName.Trim();
        settings.TimeZoneId = model.TimeZoneId;
        settings.BaseUrl = string.IsNullOrWhiteSpace(model.BaseUrl) ? null : model.BaseUrl.Trim().TrimEnd('/');
        settings.BringInAfterHour = model.BringInAfterHour;
        settings.FeedOccurrencesPerBin = model.FeedOccurrencesPerBin;
        settings.HistoryOccurrencesPerBin = model.HistoryOccurrencesPerBin;

        await _context.SaveChangesAsync();

        TempData["SettingsSaved"] = "Settings saved.";
        return RedirectToAction(nameof(Index));
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
