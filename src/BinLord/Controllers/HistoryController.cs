using System.Text;
using BinLord.Data;
using BinLord.Infrastructure;
using BinLord.Models;
using BinLord.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinLord.Controllers;

public class HistoryController : Controller
{
    private readonly BinLordContext _context;
    private readonly SettingsService _settingsService;

    public HistoryController(BinLordContext context, SettingsService settingsService)
    {
        _context = context;
        _settingsService = settingsService;
    }

    [AllowAnonymous]
    [TypeFilter(typeof(ViewingAccessFilter))]
    public async Task<IActionResult> Index()
    {
        var settings = await _settingsService.GetAsync();
        var ordered = await BuildHistoryItemsAsync(settings.HistoryOccurrencesPerBin, settings.GetToday());
        return View(ordered);
    }

    // GET: History/Export
    [AllowAnonymous]
    [TypeFilter(typeof(ViewingAccessFilter))]
    public async Task<IActionResult> Export()
    {
        var settings = await _settingsService.GetAsync();
        var ordered = await BuildHistoryItemsAsync(int.MaxValue, settings.GetToday());

        var sb = new StringBuilder();
        sb.AppendLine("Bin Name,Date,Status");
        foreach (var item in ordered)
        {
            var status = item.Status?.ToString() ?? "Not marked";
            sb.AppendLine($"{CsvEscape(item.Name)},{item.CollectionDate:yyyy-MM-dd},{status}");
        }

        var bytes = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"binlord-history-{DateTime.Now:yyyy-MM-dd}.csv");
    }

    private async Task<List<HistoryItemViewModel>> BuildHistoryItemsAsync(int maxOccurrencesPerBin, DateOnly today)
    {
        var schedules = await _context.BinSchedules.AsNoTracking().ToListAsync();
        var records = await _context.BinCollectionRecords.AsNoTracking().ToListAsync();
        var recordLookup = records.ToDictionary(r => (r.BinScheduleId, r.CollectionDate), r => r.Status);

        var items = new List<HistoryItemViewModel>();
        foreach (var schedule in schedules)
        {
            foreach (var date in schedule.GetPastOccurrences(today, maxOccurrencesPerBin))
            {
                recordLookup.TryGetValue((schedule.Id, date), out var status);
                items.Add(new HistoryItemViewModel
                {
                    BinScheduleId = schedule.Id,
                    Name = schedule.Name,
                    Colour = schedule.Colour,
                    CollectionDate = date,
                    Status = status,
                });
            }
        }

        return items
            .OrderByDescending(i => i.CollectionDate)
            .ThenBy(i => i.Name)
            .ToList();
    }

    private static string CsvEscape(string value)
    {
        return value.Contains(',') || value.Contains('"') || value.Contains('\n')
            ? "\"" + value.Replace("\"", "\"\"") + "\""
            : value;
    }

    [Authorize(Roles = "Admin,Editor")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Mark(int binScheduleId, DateOnly collectionDate, CollectionStatus status)
    {
        var record = await _context.BinCollectionRecords
            .FirstOrDefaultAsync(r => r.BinScheduleId == binScheduleId && r.CollectionDate == collectionDate);

        if (record is null)
        {
            _context.BinCollectionRecords.Add(new BinCollectionRecord
            {
                BinScheduleId = binScheduleId,
                CollectionDate = collectionDate,
                Status = status,
            });
        }
        else
        {
            record.Status = status;
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
