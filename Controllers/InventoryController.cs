using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryApp.Web.Data;

namespace InventoryApp.Web.Controllers;

public class InventoryController : Controller
{
    private readonly AppDbContext _db;

    public InventoryController(AppDbContext db)
    {
        _db = db;
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
}
