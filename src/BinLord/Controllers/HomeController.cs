using System.Diagnostics;
using BinLord.Data;
using BinLord.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinLord.Controllers;

public class HomeController : Controller
{
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

        var schedules = await _context.BinSchedules.AsNoTracking().ToListAsync();

        var upcoming = schedules
            .Select(s => new UpcomingCollectionViewModel
            {
                Id = s.Id,
                Name = s.Name,
                Colour = s.Colour,
                Notes = s.Notes,
                NextCollectionDate = s.GetNextCollectionDate(today),
            })
            .Select(vm =>
            {
                vm.DaysUntil = vm.NextCollectionDate.DayNumber - today.DayNumber;
                return vm;
            })
            .OrderBy(vm => vm.NextCollectionDate)
            .ToList();

        return View(upcoming);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
