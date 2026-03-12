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

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login", "Account");

        var canWrite = await HasWriteAccessAsync(inventory, userId);
        if (!canWrite) return Forbid();

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

        var canWrite = await HasWriteAccessAsync(inventory, userId);
        if (!canWrite) return Forbid();

        // Автогенерация CustomId, если не задан
        if (string.IsNullOrWhiteSpace(customId))
        {
            var prefix = $"INV-{inventory.Id}-";
            var existingIds = await _db.Items
                .Where(i => i.InventoryId == inventory.Id && i.CustomId != null && i.CustomId.StartsWith(prefix))
                .Select(i => i.CustomId!)
                .ToListAsync();

            int maxSeq = 0;
            foreach (var idStr in existingIds)
            {
                var tail = idStr.Substring(prefix.Length);
                if (int.TryParse(tail, out var n) && n > maxSeq)
                    maxSeq = n;
            }

            var next = maxSeq + 1;
            customId = prefix + next.ToString("D6");
        }

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
            .Include(i => i.CreatedBy)
            .Include(i => i.Likes)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        ViewBag.UserId = userId;

        var canEdit = item.Inventory != null
            ? await HasWriteAccessAsync(item.Inventory, userId)
            : false;
        ViewBag.CanEdit = canEdit;

        var liked = false;
        if (!string.IsNullOrEmpty(userId))
            liked = item.Likes.Any(l => l.UserId == userId);
        ViewBag.Liked = liked;
        ViewBag.LikeCount = item.Likes.Count;

        return View(item);
    }

    // GET: /Item/Edit/{id}
    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _db.Items
            .Include(i => i.Inventory)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (item == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        if (item.Inventory == null || !await HasWriteAccessAsync(item.Inventory, userId))
            return Forbid();

        return View(item);
    }

    // POST: /Item/Edit/{id}
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        string? customId,
        string? customString1,
        int? customInt1,
        bool? customBool1,
        int version)
    {
        var item = await _db.Items
            .Include(i => i.Inventory)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (item == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        if (item.Inventory == null || !await HasWriteAccessAsync(item.Inventory, userId))
            return Forbid();

        // Оптимистичная блокировка для item
        if (item.Version != version)
            return BadRequest("Conflict: item was modified by someone else");

        item.CustomId = string.IsNullOrWhiteSpace(customId) ? item.CustomId : customId;
        item.CustomString1 = customString1;
        item.CustomInt1 = customInt1;
        item.CustomBool1 = customBool1;
        item.Version++;
        item.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return RedirectToAction("Details", new { id = item.Id });
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
        var canWrite = item.Inventory != null
            ? await HasWriteAccessAsync(item.Inventory, userId)
            : false;
        if (!canWrite)
            return Forbid();

        var inventoryId = item.InventoryId;
        _db.Items.Remove(item);
        await _db.SaveChangesAsync();

        return RedirectToAction("Details", "Inventory", new { id = inventoryId, tab = "items" });
    }

    private async Task<bool> HasWriteAccessAsync(Inventory inventory, string? userId)
    {
        if (string.IsNullOrEmpty(userId))
            return false;

        if (User.IsInRole("Admin") || inventory.CreatorId == userId)
            return true;

        if (inventory.IsPublic)
            return true;

        return await _db.InventoryAccesses
            .AnyAsync(a => a.InventoryId == inventory.Id && a.UserId == userId);
    }
}

