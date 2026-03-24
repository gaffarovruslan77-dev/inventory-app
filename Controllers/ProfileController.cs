using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryApp.Web.Data;
using InventoryApp.Web.Models;
using InventoryApp.Web.Services;

namespace InventoryApp.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;
    private readonly ISalesforceService _salesforce;

    public ProfileController(
        AppDbContext db,
        UserManager<AppUser> userManager,
        ISalesforceService salesforce)
    {
        _db = db;
        _userManager = userManager;
        _salesforce = salesforce;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login", "Account");

        ViewBag.ShowAddToCrm = _salesforce.IsConfigured;

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCrm([FromBody] AddToCrmRequest body, CancellationToken cancellationToken)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Json(new { success = false, error = "Not signed in." });

        if (!_salesforce.IsConfigured)
            return Json(new { success = false, error = "Salesforce is not configured." });

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Json(new { success = false, error = "User not found." });

        if (string.IsNullOrWhiteSpace(user.Email))
            return Json(new { success = false, error = "Your account has no email; cannot create Contact." });

        var result = await _salesforce.CreateAccountAndContactAsync(
            body.Company ?? "",
            user.Email,
            user.DisplayName,
            body.Phone,
            body.JobTitle,
            cancellationToken);

        if (!result.Success)
            return Json(new { success = false, error = result.ErrorMessage ?? "Salesforce error." });

        return Json(new { success = true });
    }

    public class AddToCrmRequest
    {
        public string? Company { get; set; }
        public string? Phone { get; set; }
        public string? JobTitle { get; set; }
    }

    public class ProfileViewModel
    {
        public AppUser User { get; set; } = null!;
        public List<Inventory> OwnedInventories { get; set; } = new();
        public List<Inventory> WritableInventories { get; set; } = new();
    }
}

