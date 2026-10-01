using System.Text;
using System.Xml.Linq;
using BinLord.Data;
using BinLord.Models;
using BinLord.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinLord.Controllers;

[AllowAnonymous]
public class FeedController : Controller
{
    private readonly BinLordContext _context;
    private readonly SettingsService _settingsService;

    public FeedController(BinLordContext context, SettingsService settingsService)
    {
        _context = context;
        _settingsService = settingsService;
    }

    [Route("feed.xml")]
    public async Task<IActionResult> Rss(string? token)
    {
        var settings = await _settingsService.GetAsync();
        if (!User.Identity!.IsAuthenticated && !settings.AllowsAnonymousFeedAccess(token))
        {
            return Unauthorized();
        }

        var today = settings.GetToday();
        var schedules = await _context.BinSchedules.AsNoTracking().OrderBy(s => s.Name).ToListAsync();
        var baseUrl = settings.GetEffectiveBaseUrl($"{Request.Scheme}://{Request.Host}");

        var items = schedules
            .SelectMany(schedule => schedule.GetUpcomingOccurrences(today, settings.FeedOccurrencesPerBin)
                .Select(date => (schedule, date)))
            .OrderBy(x => x.date)
            .ThenBy(x => x.schedule.Name)
            .Select(x => BuildItem(x.schedule, x.date, baseUrl));

        var channel = new XElement(
            "channel",
            new XElement("title", $"{settings.AppName} — Bin Collection Schedule"),
            new XElement("link", baseUrl + "/"),
            new XElement("description", $"Upcoming bin collections from {settings.AppName}."),
            new XElement("lastBuildDate", DateTime.UtcNow.ToString("r")),
            items);

        var document = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("rss", new XAttribute("version", "2.0"), channel));

        using var stream = new MemoryStream();
        await using (var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), leaveOpen: true))
        {
            await document.SaveAsync(writer, SaveOptions.None, HttpContext.RequestAborted);
        }

        return File(stream.ToArray(), "application/rss+xml; charset=utf-8");
    }

    private static XElement BuildItem(BinSchedule schedule, DateOnly date, string baseUrl)
    {
        var description = string.IsNullOrWhiteSpace(schedule.Notes)
            ? $"{schedule.Name} is collected on {date:dddd, d MMMM yyyy}."
            : $"{schedule.Name} is collected on {date:dddd, d MMMM yyyy}. {schedule.Notes}";

        return new XElement(
            "item",
            new XElement("title", $"{schedule.Name} collection – {date:dddd d MMMM yyyy}"),
            new XElement("link", $"{baseUrl}/BinSchedules/Details/{schedule.Id}"),
            new XElement("guid", new XAttribute("isPermaLink", "false"), $"binlord-{schedule.Id}-{date:yyyy-MM-dd}"),
            new XElement("pubDate", date.ToDateTime(TimeOnly.MinValue).ToString("r")),
            new XElement("description", description));
    }
}
