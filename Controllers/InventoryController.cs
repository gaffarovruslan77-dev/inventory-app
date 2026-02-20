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
            return Content("User ID is null - not logged in properly");

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
        return RedirectToAction("Index");
    }
}
