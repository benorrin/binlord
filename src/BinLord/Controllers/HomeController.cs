using System.Diagnostics;
using BinLord.Data;
using BinLord.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinLord.Controllers;

public class HomeController : Controller
{
    // After this hour, a bin that's been put out but not brought in is
    // treated as needing to come back in rather than just "out".
    private const int BringInAfterHour = 14;

    private readonly ILogger<HomeController> _logger;
    private readonly BinLordContext _context;

    public HomeController(ILogger<HomeController> logger, BinLordContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var now = DateTime.Now;

        var schedules = await _context.BinSchedules.AsNoTracking().ToListAsync();
        var todaysRecords = await _context.BinCollectionRecords
            .AsNoTracking()
            .Where(r => r.CollectionDate == today)
            .ToDictionaryAsync(r => r.BinScheduleId);

        var upcoming = schedules
            .Select(s => new UpcomingCollectionViewModel
            {
                Id = s.Id,
                Name = s.Name,
                Colour = s.Colour,
                Notes = s.Notes,
                NextCollectionDate = s.GetNextCollectionDate(today),
            })
            .ToList();

        foreach (var item in upcoming)
        {
            item.DaysUntil = item.NextCollectionDate.DayNumber - today.DayNumber;

            if (item.IsToday && todaysRecords.TryGetValue(item.Id, out var record))
            {
                item.TakenOutAt = record.TakenOutAt;
                item.BroughtInAt = record.BroughtInAt;
            }

            item.ActionStage = item.BroughtInAt is not null
                ? BinActionStage.Done
                : item.TakenOutAt is not null
                    ? (now.Hour >= BringInAfterHour ? BinActionStage.NeedsBringIn : BinActionStage.PutOut)
                    : BinActionStage.NeedsPutOut;
        }

        var ordered = upcoming.OrderBy(vm => vm.NextCollectionDate).ToList();
        return View(ordered);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleTakenOut(int binScheduleId, DateOnly collectionDate)
    {
        await ToggleActionAsync(binScheduleId, collectionDate, takenOut: true);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleBroughtIn(int binScheduleId, DateOnly collectionDate)
    {
        await ToggleActionAsync(binScheduleId, collectionDate, takenOut: false);
        return RedirectToAction(nameof(Index));
    }

    private async Task ToggleActionAsync(int binScheduleId, DateOnly collectionDate, bool takenOut)
    {
        var record = await _context.BinCollectionRecords
            .FirstOrDefaultAsync(r => r.BinScheduleId == binScheduleId && r.CollectionDate == collectionDate);

        var isNew = record is null;
        record ??= new BinCollectionRecord { BinScheduleId = binScheduleId, CollectionDate = collectionDate };

        if (takenOut)
        {
            record.TakenOutAt = record.TakenOutAt is null ? DateTime.Now : null;
        }
        else
        {
            record.BroughtInAt = record.BroughtInAt is null ? DateTime.Now : null;
        }

        if (isNew)
        {
            _context.BinCollectionRecords.Add(record);
        }

        await _context.SaveChangesAsync();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
