using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryApp.Web.Data;
using InventoryApp.Web.Models;

namespace InventoryApp.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;

    public ProfileController(AppDbContext db, UserManager<AppUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login", "Account");

        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);

        var owned = await _db.Inventories
            .Where(i => i.CreatorId == userId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        var writeAccess = await _db.InventoryAccesses
            .Where(a => a.UserId == userId)
            .Select(a => a.InventoryId)
            .ToListAsync();

        var writable = await _db.Inventories
            .Where(i => i.IsPublic || writeAccess.Contains(i.Id))
            .Where(i => i.CreatorId != userId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        var vm = new ProfileViewModel
        {
            User = user!,
            OwnedInventories = owned,
            WritableInventories = writable
        };

        return View(vm);
    }

    public class ProfileViewModel
    {
        public AppUser User { get; set; } = null!;
        public List<Inventory> OwnedInventories { get; set; } = new();
        public List<Inventory> WritableInventories { get; set; } = new();
    }
}

