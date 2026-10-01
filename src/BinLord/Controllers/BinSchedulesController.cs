using System.Globalization;
using System.Text;
using System.Text.Json;
using BinLord.Data;
using BinLord.Infrastructure;
using BinLord.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinLord.Controllers;

[Authorize(Roles = "Admin,Editor")]
public class BinSchedulesController : Controller
{
    private readonly BinLordContext _context;

    public BinSchedulesController(BinLordContext context)
    {
        _context = context;
    }

    // GET: BinSchedules
    [AllowAnonymous]
    [TypeFilter(typeof(ViewingAccessFilter))]
    public async Task<IActionResult> Index()
    {
        var schedules = await _context.BinSchedules
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync();
        return View(schedules);
    }

    // GET: BinSchedules/Details/5
    [AllowAnonymous]
    [TypeFilter(typeof(ViewingAccessFilter))]
    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var schedule = await _context.BinSchedules.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        if (schedule is null)
        {
            return NotFound();
        }

        return View(schedule);
    }

    // GET: BinSchedules/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: BinSchedules/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Colour,FirstCollectionDate,FrequencyWeeks,Notes")] BinSchedule binSchedule)
    {
        if (ModelState.IsValid)
        {
            _context.Add(binSchedule);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(binSchedule);
    }

    // GET: BinSchedules/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var binSchedule = await _context.BinSchedules.FindAsync(id);
        if (binSchedule is null)
        {
            return NotFound();
        }

        return View(binSchedule);
    }

    // POST: BinSchedules/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Colour,FirstCollectionDate,FrequencyWeeks,Notes")] BinSchedule binSchedule)
    {
        if (id != binSchedule.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(binSchedule);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await _context.BinSchedules.AnyAsync(s => s.Id == id);
                if (!exists)
                {
                    return NotFound();
                }
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(binSchedule);
    }

    // GET: BinSchedules/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var binSchedule = await _context.BinSchedules.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        if (binSchedule is null)
        {
            return NotFound();
        }

        return View(binSchedule);
    }

    // POST: BinSchedules/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var binSchedule = await _context.BinSchedules.FindAsync(id);
        if (binSchedule is not null)
        {
            _context.BinSchedules.Remove(binSchedule);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: BinSchedules/Export
    public async Task<IActionResult> Export()
    {
        var schedules = await _context.BinSchedules
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync();

        var dtos = schedules.Select(s => new BinScheduleExportDto
        {
            Name = s.Name,
            Colour = s.Colour,
            FirstCollectionDate = s.FirstCollectionDate.ToString("yyyy-MM-dd"),
            FrequencyWeeks = s.FrequencyWeeks,
            Notes = s.Notes,
        }).ToList();

        var json = JsonSerializer.Serialize(dtos, new JsonSerializerOptions { WriteIndented = true });
        var bytes = Encoding.UTF8.GetBytes(json);
        return File(bytes, "application/json", $"binlord-schedules-{DateTime.Now:yyyy-MM-dd}.json");
    }

    // POST: BinSchedules/Import
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            TempData["ImportError"] = "Please choose a JSON file to import.";
            return RedirectToAction(nameof(Index));
        }

        List<BinScheduleExportDto>? dtos;
        try
        {
            using var stream = file.OpenReadStream();
            dtos = await JsonSerializer.DeserializeAsync<List<BinScheduleExportDto>>(
                stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException)
        {
            TempData["ImportError"] = "That file doesn't look like a valid BinLord export.";
            return RedirectToAction(nameof(Index));
        }

        if (dtos is null || dtos.Count == 0)
        {
            TempData["ImportError"] = "No bin schedules were found in that file.";
            return RedirectToAction(nameof(Index));
        }

        var imported = 0;
        var errors = new List<string>();

        for (var i = 0; i < dtos.Count; i++)
        {
            var dto = dtos[i];
            var rowLabel = string.IsNullOrWhiteSpace(dto.Name) ? $"Row {i + 1}" : dto.Name;

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                errors.Add($"{rowLabel}: missing a name.");
                continue;
            }

            if (!DateOnly.TryParseExact(dto.FirstCollectionDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                errors.Add($"{rowLabel}: invalid date '{dto.FirstCollectionDate}' (expected yyyy-MM-dd).");
                continue;
            }

            if (dto.FrequencyWeeks is < 1 or > 52)
            {
                errors.Add($"{rowLabel}: frequency must be between 1 and 52 weeks.");
                continue;
            }

            _context.BinSchedules.Add(new BinSchedule
            {
                Name = dto.Name.Trim(),
                Colour = string.IsNullOrWhiteSpace(dto.Colour) ? "#4caf50" : dto.Colour,
                FirstCollectionDate = date,
                FrequencyWeeks = dto.FrequencyWeeks,
                Notes = dto.Notes,
            });
            imported++;
        }

        if (imported > 0)
        {
            await _context.SaveChangesAsync();
            TempData["ImportSuccess"] = $"Imported {imported} bin schedule{(imported == 1 ? "" : "s")}.";
        }

        if (errors.Count > 0)
        {
            var shown = errors.Take(5);
            var suffix = errors.Count > 5 ? $" (+{errors.Count - 5} more)" : "";
            TempData["ImportError"] = string.Join(" ", shown) + suffix;
        }

        return RedirectToAction(nameof(Index));
    }
}
