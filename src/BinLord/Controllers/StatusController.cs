using BinLord.Models;
using BinLord.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BinLord.Controllers;

/// <summary>
/// Lightweight endpoints for uptime/health monitoring tools such as
/// Uptime Kuma: a plain liveness check, and a status check that reports
/// whether any bin currently needs putting out or bringing in. Always
/// anonymous — that's the whole point of unattended monitoring — even
/// when schedules otherwise require login to view.
/// </summary>
[AllowAnonymous]
public class StatusController : Controller
{
    private readonly SettingsService _settingsService;
    private readonly BinStatusService _binStatusService;

    public StatusController(SettingsService settingsService, BinStatusService binStatusService)
    {
        _settingsService = settingsService;
        _binStatusService = binStatusService;
    }

    [Route("health")]
    public IActionResult Health() => Content("OK", "text/plain");

    [Route("api/status")]
    public async Task<IActionResult> Status()
    {
        var settings = await _settingsService.GetAsync();
        var upcoming = await _binStatusService.GetUpcomingAsync(settings);

        var pending = upcoming
            .Where(u => u.IsToday && u.ActionStage is BinActionStage.NeedsPutOut or BinActionStage.NeedsBringIn)
            .Select(u => new
            {
                name = u.Name,
                stage = u.ActionStage.ToString(),
            })
            .ToList();

        return Json(new
        {
            ok = pending.Count == 0,
            pendingCount = pending.Count,
            pending,
            checkedAt = settings.GetNow(),
        });
    }
}
