using System.Text;
using BinLord.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinLord.Controllers;

public class CalendarController : Controller
{
    private readonly BinLordContext _context;

    public CalendarController(BinLordContext context)
    {
        _context = context;
    }

    [Route("calendar.ics")]
    public async Task<IActionResult> Ics()
    {
        var schedules = await _context.BinSchedules.AsNoTracking().OrderBy(s => s.Name).ToListAsync();
        var dtStamp = DateTime.UtcNow.ToString("yyyyMMdd'T'HHmmss'Z'");

        var lines = new List<string>
        {
            "BEGIN:VCALENDAR",
            "VERSION:2.0",
            "PRODID:-//BinLord//Bin Collection Schedule//EN",
            "CALSCALE:GREGORIAN",
            "METHOD:PUBLISH",
            "X-WR-CALNAME:BinLord — Bin Collection Schedule",
        };

        foreach (var schedule in schedules)
        {
            lines.Add("BEGIN:VEVENT");
            lines.Add($"UID:binlord-{schedule.Id}@binlord.local");
            lines.Add($"DTSTAMP:{dtStamp}");
            lines.Add($"DTSTART;VALUE=DATE:{schedule.FirstCollectionDate:yyyyMMdd}");
            lines.Add($"RRULE:FREQ=WEEKLY;INTERVAL={schedule.FrequencyWeeks}");
            lines.Add($"SUMMARY:{EscapeText(schedule.Name + " collection")}");
            if (!string.IsNullOrWhiteSpace(schedule.Notes))
            {
                lines.Add($"DESCRIPTION:{EscapeText(schedule.Notes)}");
            }
            lines.Add("END:VEVENT");
        }

        lines.Add("END:VCALENDAR");

        var content = string.Join("\r\n", lines.Select(FoldLine)) + "\r\n";
        var bytes = Encoding.UTF8.GetBytes(content);
        return File(bytes, "text/calendar; charset=utf-8");
    }

    private static string EscapeText(string value) =>
        value.Replace("\\", "\\\\").Replace(";", "\\;").Replace(",", "\\,").Replace("\r\n", "\\n").Replace("\n", "\\n");

    private static string FoldLine(string line)
    {
        const int maxLen = 73;
        if (line.Length <= maxLen)
        {
            return line;
        }

        var sb = new StringBuilder();
        var first = true;
        var remaining = line;
        while (remaining.Length > 0)
        {
            var take = Math.Min(first ? maxLen : maxLen - 1, remaining.Length);
            if (!first)
            {
                sb.Append("\r\n ");
            }
            sb.Append(remaining[..take]);
            remaining = remaining[take..];
            first = false;
        }

        return sb.ToString();
    }
}
