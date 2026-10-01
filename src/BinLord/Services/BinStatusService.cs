using BinLord.Data;
using BinLord.Models;
using Microsoft.EntityFrameworkCore;

namespace BinLord.Services;

/// <summary>
/// Computes each bin's next collection date and same-day action stage.
/// Shared by the homepage, the reminder background service, and the
/// Uptime-Kuma-friendly status endpoint, so they all agree on what
/// "needs putting out" / "needs bringing in" means.
/// </summary>
public class BinStatusService
{
    private readonly BinLordContext _context;

    public BinStatusService(BinLordContext context)
    {
        _context = context;
    }

    public async Task<List<UpcomingCollectionViewModel>> GetUpcomingAsync(AppSettings settings)
    {
        var today = settings.GetToday();
        var now = settings.GetNow();

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
                    ? (now.Hour >= settings.BringInAfterHour ? BinActionStage.NeedsBringIn : BinActionStage.PutOut)
                    : BinActionStage.NeedsPutOut;
        }

        return upcoming.OrderBy(vm => vm.NextCollectionDate).ToList();
    }
}
