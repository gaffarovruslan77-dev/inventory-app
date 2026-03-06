using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryApp.Web.Data;
using InventoryApp.Web.Models;

namespace InventoryApp.Web.Controllers;

public class ItemController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;

    public ItemController(AppDbContext db, UserManager<AppUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // GET: /Item/Create/{id}  (id = inventoryId)
    [Authorize]
    public async Task<IActionResult> Create(int id)
    {
        var inventory = await _db.Inventories.FindAsync(id);
        if (inventory == null) return NotFound();

        ViewBag.InventoryId = id;
        return View(inventory);
    }

    // POST: /Item/Create/{id}
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        int id,
        string? customId,
        string? customString1,
        int? customInt1,
        bool? customBool1)
    {
        var inventory = await _db.Inventories.FindAsync(id);
        if (inventory == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login", "Account");

        var item = new Item
        {
            InventoryId = inventory.Id,
            CreatedById = userId,
            CustomId = customId,
            CustomString1 = customString1,
            CustomInt1 = customInt1,
            CustomBool1 = customBool1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Items.Add(item);
        await _db.SaveChangesAsync();

        return RedirectToAction("Details", "Inventory", new { id = inventory.Id, tab = "items" });
    }

    // GET: /Item/Details/{id}
    public async Task<IActionResult> Details(int id)
    {
        var item = await _db.Items
            .Include(i => i.Inventory)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        ViewBag.UserId = userId;

        return View(item);
    }

    // POST: /Item/Delete/{id}
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.Items
            .Include(i => i.Inventory)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        // Разрешаем удалять создателю или владельцу инвентаря
        if (item.CreatedById != userId && item.Inventory?.CreatorId != userId)
            return Forbid();

        var inventoryId = item.InventoryId;
        _db.Items.Remove(item);
        await _db.SaveChangesAsync();

        return RedirectToAction("Details", "Inventory", new { id = inventoryId, tab = "items" });
    }
}

