using BinLord.Data;
using BinLord.Models;
using Microsoft.EntityFrameworkCore;

namespace BinLord.Services;

/// <summary>
/// Reads (and lazily creates) the single <see cref="AppSettings"/> row.
/// </summary>
public class SettingsService
{
    private readonly BinLordContext _context;

    public SettingsService(BinLordContext context)
    {
        _context = context;
    }

    public async Task<AppSettings> GetAsync()
    {
        var settings = await _context.AppSettings.FirstOrDefaultAsync();
        if (settings is null)
        {
            settings = new AppSettings();
            _context.AppSettings.Add(settings);
            await _context.SaveChangesAsync();
        }

        return settings;
    }
}
