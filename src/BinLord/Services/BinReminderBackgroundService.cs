using BinLord.Data;
using BinLord.Models;
using Microsoft.EntityFrameworkCore;

namespace BinLord.Services;

/// <summary>
/// Periodically checks for bins that need putting out or bringing in and,
/// if email/ntfy notifications are enabled, sends a reminder once per
/// occurrence (tracked on <see cref="BinCollectionRecord"/> so it isn't
/// repeated on every poll).
/// </summary>
public class BinReminderBackgroundService : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(15);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BinReminderBackgroundService> _logger;

    public BinReminderBackgroundService(IServiceScopeFactory scopeFactory, ILogger<BinReminderBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(CheckInterval);
        do
        {
            try
            {
                await CheckAndNotifyAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Bin reminder check failed.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task CheckAndNotifyAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BinLordContext>();
        var settingsService = scope.ServiceProvider.GetRequiredService<SettingsService>();
        var binStatusService = scope.ServiceProvider.GetRequiredService<BinStatusService>();
        var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();

        var settings = await settingsService.GetAsync();
        if (!settings.EmailNotificationsEnabled && !settings.NtfyEnabled)
        {
            return;
        }

        var upcoming = await binStatusService.GetUpcomingAsync(settings);
        var today = settings.GetToday();

        foreach (var item in upcoming.Where(u => u.IsToday))
        {
            if (item.ActionStage == BinActionStage.NeedsPutOut)
            {
                await NotifyOnceAsync(
                    context,
                    notificationService,
                    settings,
                    item.Id,
                    today,
                    isPutOut: true,
                    title: $"Put out: {item.Name}",
                    body: $"{item.Name} is collected today — don't forget to put it out.",
                    tags: "wastebasket",
                    ct);
            }
            else if (item.ActionStage == BinActionStage.NeedsBringIn)
            {
                await NotifyOnceAsync(
                    context,
                    notificationService,
                    settings,
                    item.Id,
                    today,
                    isPutOut: false,
                    title: $"Bring in: {item.Name}",
                    body: $"{item.Name} should have been collected by now — bring the bin back in.",
                    tags: "house",
                    ct);
            }
        }
    }

    private static async Task NotifyOnceAsync(
        BinLordContext context,
        NotificationService notifier,
        AppSettings settings,
        int binScheduleId,
        DateOnly date,
        bool isPutOut,
        string title,
        string body,
        string tags,
        CancellationToken ct)
    {
        var record = await context.BinCollectionRecords
            .FirstOrDefaultAsync(r => r.BinScheduleId == binScheduleId && r.CollectionDate == date, ct);

        var alreadySent = isPutOut ? record?.PutOutReminderSentAt is not null : record?.BringInReminderSentAt is not null;
        if (alreadySent)
        {
            return;
        }

        var outcome = await notifier.SendAsync(settings, title, body, tags);
        if (!outcome.Email.Success && !outcome.Ntfy.Success)
        {
            // Nothing got through; try again next poll rather than marking as sent.
            return;
        }

        var isNew = record is null;
        record ??= new BinCollectionRecord { BinScheduleId = binScheduleId, CollectionDate = date };

        if (isPutOut)
        {
            record.PutOutReminderSentAt = settings.GetNow();
        }
        else
        {
            record.BringInReminderSentAt = settings.GetNow();
        }

        if (isNew)
        {
            context.BinCollectionRecords.Add(record);
        }

        await context.SaveChangesAsync(ct);
    }
}
