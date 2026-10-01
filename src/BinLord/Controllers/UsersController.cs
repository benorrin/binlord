using BinLord.Data;
using BinLord.Models;
using BinLord.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinLord.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly BinLordContext _context;
    private readonly UserService _userService;

    public UsersController(BinLordContext context, UserService userService)
    {
        _context = context;
        _userService = userService;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _context.Users.AsNoTracking().OrderBy(u => u.Username).ToListAsync();
        return View(users);
    }

    public IActionResult Create() => View(new CreateUserViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (await _userService.FindByUsernameAsync(model.Username.Trim()) is not null)
        {
            ModelState.AddModelError(nameof(model.Username), "That username is already taken.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _userService.CreateAsync(model.Username.Trim(), model.Password, model.Role);
        TempData["UsersMessage"] = $"Created user \"{model.Username.Trim()}\".";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var user = await _context.Users.FindAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        return View(new EditUserViewModel { Id = user.Id, Username = user.Username, Role = user.Role });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditUserViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        var user = await _context.Users.FindAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var existingWithUsername = await _userService.FindByUsernameAsync(model.Username.Trim());
        if (existingWithUsername is not null && existingWithUsername.Id != id)
        {
            ModelState.AddModelError(nameof(model.Username), "That username is already taken.");
        }

        if (user.Role == UserRole.Admin && model.Role != UserRole.Admin && await _userService.CountAdminsAsync() <= 1)
        {
            ModelState.AddModelError(nameof(model.Role), "There must be at least one admin — promote another user first.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        user.Username = model.Username.Trim();
        user.Role = model.Role;
        if (!string.IsNullOrWhiteSpace(model.NewPassword))
        {
            user.PasswordHash = PasswordHasher.Hash(model.NewPassword);
        }

        await _context.SaveChangesAsync();
        TempData["UsersMessage"] = $"Updated user \"{user.Username}\".";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (user is null)
        {
            return NotFound();
        }

        return View(user);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user is null)
        {
            return RedirectToAction(nameof(Index));
        }

        if (user.Role == UserRole.Admin && await _userService.CountAdminsAsync() <= 1)
        {
            TempData["UsersError"] = "Can't delete the last admin.";
            return RedirectToAction(nameof(Index));
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        TempData["UsersMessage"] = $"Deleted user \"{user.Username}\".";
        return RedirectToAction(nameof(Index));
    }
}
