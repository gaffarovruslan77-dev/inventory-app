using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryApp.Web.Data;
using InventoryApp.Web.Models;

namespace InventoryApp.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _db;

    public HomeController(ILogger<HomeController> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var latest = await _db.Inventories
            .Include(i => i.Creator)
            .OrderByDescending(i => i.CreatedAt)
            .Take(10)
            .ToListAsync();

        var top = await _db.Inventories
            .OrderByDescending(i => i.Items.Count)
            .Include(i => i.Creator)
            .Take(5)
            .ToListAsync();

        var tags = await _db.Tags
            .OrderBy(t => t.Name)
            .Take(50)
            .Select(t => t.Name)
            .ToListAsync();

        return View(new HomeViewModel
        {
            LatestInventories = latest,
            TopInventories = top,
            Tags = tags
        });
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public class HomeViewModel
    {
        public List<Inventory> LatestInventories { get; set; } = new();
        public List<Inventory> TopInventories { get; set; } = new();
        public List<string> Tags { get; set; } = new();
    }
}
