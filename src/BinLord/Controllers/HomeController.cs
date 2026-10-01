using System.Diagnostics;
using BinLord.Data;
using BinLord.Models;
using BinLord.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinLord.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly BinLordContext _context;
    private readonly SettingsService _settingsService;
    private readonly BinStatusService _binStatusService;

    public HomeController(ILogger<HomeController> logger, BinLordContext context, SettingsService settingsService, BinStatusService binStatusService)
    {
        _logger = logger;
        _context = context;
        _settingsService = settingsService;
        _binStatusService = binStatusService;
    }

    public async Task<IActionResult> Index()
    {
        var settings = await _settingsService.GetAsync();
        var upcoming = await _binStatusService.GetUpcomingAsync(settings);
        return View(upcoming);
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
        var settings = await _settingsService.GetAsync();
        var record = await _context.BinCollectionRecords
            .FirstOrDefaultAsync(r => r.BinScheduleId == binScheduleId && r.CollectionDate == collectionDate);

        var isNew = record is null;
        record ??= new BinCollectionRecord { BinScheduleId = binScheduleId, CollectionDate = collectionDate };

        if (takenOut)
        {
            record.TakenOutAt = record.TakenOutAt is null ? settings.GetNow() : null;
        }
        else
        {
            record.BroughtInAt = record.BroughtInAt is null ? settings.GetNow() : null;
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
