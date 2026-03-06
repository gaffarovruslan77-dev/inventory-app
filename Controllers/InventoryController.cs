using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using InventoryApp.Web.Data;
using InventoryApp.Web.Models;

namespace InventoryApp.Web.Controllers;

public class InventoryController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;

    public InventoryController(AppDbContext db, UserManager<AppUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var inventories = await _db.Inventories
            .Include(i => i.Creator)
            .Include(i => i.Category)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
        return View(inventories);
    }

    public async Task<IActionResult> Details(int id, string tab = "items")
    {
        var inventory = await _db.Inventories
            .Include(i => i.Creator)
            .Include(i => i.Category)
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (inventory == null) return NotFound();

        ViewBag.Tab = tab;
        ViewBag.UserId = _userManager.GetUserId(User);
        return View(inventory);
    }

    [Authorize]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> Create(string title, string? description)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login", "Account");

        var inventory = new Inventory
        {
            Title = title,
            Description = description,
            CreatorId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Inventories.Add(inventory);
        await _db.SaveChangesAsync();
        return RedirectToAction("Details", new { id = inventory.Id });
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> SaveSettings(int id, string title, 
        string? description, bool isPublic, int version)
    {
        var inventory = await _db.Inventories.FindAsync(id);
        if (inventory == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        if (inventory.CreatorId != userId) return Forbid();

        // Оптимистичная блокировка
        if (inventory.Version != version)
            return BadRequest("Conflict: inventory was modified by someone else");

        inventory.Title = title;
        inventory.Description = description;
        inventory.IsPublic = isPublic;
        inventory.UpdatedAt = DateTime.UtcNow;
        inventory.Version++;

        await _db.SaveChangesAsync();
        return Json(new { success = true, version = inventory.Version });
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var inventory = await _db.Inventories.FindAsync(id);
        if (inventory == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        if (inventory.CreatorId != userId) return Forbid();

        _db.Inventories.Remove(inventory);
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }
}
