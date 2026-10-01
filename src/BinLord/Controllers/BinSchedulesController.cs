using BinLord.Data;
using BinLord.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinLord.Controllers;

public class BinSchedulesController : Controller
{
    private readonly BinLordContext _context;

    public BinSchedulesController(BinLordContext context)
    {
        _context = context;
    }

    // GET: BinSchedules
    public async Task<IActionResult> Index()
    {
        var schedules = await _context.BinSchedules
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync();
        return View(schedules);
    }

    // GET: BinSchedules/Details/5
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
}
