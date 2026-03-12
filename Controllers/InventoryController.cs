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

    private string? CurrentUserId => _userManager.GetUserId(User);

    private bool IsAdmin() => User.IsInRole("Admin");

    private bool IsOwner(Inventory inventory, string? userId) =>
        !string.IsNullOrEmpty(userId) && inventory.CreatorId == userId;

    private bool CanManageInventory(Inventory inventory, string? userId) =>
        IsAdmin() || IsOwner(inventory, userId);

    private async Task<bool> HasWriteAccessAsync(Inventory inventory, string? userId)
    {
        if (string.IsNullOrEmpty(userId))
            return false;

        if (IsAdmin() || inventory.CreatorId == userId)
            return true;

        if (inventory.IsPublic)
            return true;

        return await _db.InventoryAccesses
            .AnyAsync(a => a.InventoryId == inventory.Id && a.UserId == userId);
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
            .Include(i => i.AccessList)
            .ThenInclude(a => a.User)
            .Include(i => i.Tags)
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (inventory == null) return NotFound();

        var canWrite = await HasWriteAccessAsync(inventory, CurrentUserId);

        ViewBag.Tab = tab;
        ViewBag.UserId = CurrentUserId;
        ViewBag.CanWrite = canWrite;
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
    public async Task<IActionResult> SaveSettings([FromBody] SaveSettingsDto dto)
    {
        var inventory = await _db.Inventories.FindAsync(dto.Id);
        if (inventory == null) return NotFound();

        var userId = CurrentUserId;
        if (!CanManageInventory(inventory, userId)) return Forbid();

        // Оптимистичная блокировка
        if (inventory.Version != dto.Version)
            return BadRequest("Conflict: inventory was modified by someone else");

        inventory.Title = dto.Title;
        inventory.Description = dto.Description;
        inventory.IsPublic = dto.IsPublic;
        inventory.UpdatedAt = DateTime.UtcNow;
        inventory.Version++;

        await _db.SaveChangesAsync();
        return Json(new { success = true, version = inventory.Version });
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> SaveFields([FromBody] InventoryFieldsDto dto)
    {
        var inventory = await _db.Inventories.FindAsync(dto.Id);
        if (inventory == null) return NotFound();

        var userId = CurrentUserId;
        if (!CanManageInventory(inventory, userId)) return Forbid();

        // Оптимистичная блокировка для полей
        if (inventory.Version != dto.Version)
            return BadRequest("Conflict: inventory was modified by someone else");

        inventory.CustomString1Name = dto.CustomString1Name;
        inventory.CustomString1State = dto.CustomString1State;
        inventory.CustomInt1Name = dto.CustomInt1Name;
        inventory.CustomInt1State = dto.CustomInt1State;
        inventory.CustomBool1Name = dto.CustomBool1Name;
        inventory.CustomBool1State = dto.CustomBool1State;

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

        var userId = CurrentUserId;
        if (!CanManageInventory(inventory, userId)) return Forbid();

        _db.Inventories.Remove(inventory);
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    [HttpGet, Authorize]
    public async Task<IActionResult> SearchUsers(string term)
    {
        term = term?.Trim() ?? string.Empty;
        if (term.Length < 2)
            return Json(Array.Empty<object>());

        var query = _db.Users.AsQueryable();

        query = query.Where(u =>
            (u.DisplayName != null && EF.Functions.ILike(u.DisplayName, $"{term}%")) ||
            (u.Email != null && EF.Functions.ILike(u.Email, $"{term}%")));

        var users = await query
            .OrderBy(u => u.DisplayName ?? u.Email)
            .Take(10)
            .Select(u => new
            {
                id = u.Id,
                name = u.DisplayName ?? u.UserName ?? u.Email,
                email = u.Email
            })
            .ToListAsync();

        return Json(users);
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> AddAccess(int inventoryId, string userId)
    {
        var inventory = await _db.Inventories
            .Include(i => i.AccessList)
            .FirstOrDefaultAsync(i => i.Id == inventoryId);
        if (inventory == null) return NotFound();

        var currentUserId = CurrentUserId;
        if (!CanManageInventory(inventory, currentUserId)) return Forbid();

        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("User id is required");

        if (inventory.CreatorId == userId)
            return BadRequest("Owner already has full access");

        var exists = inventory.AccessList.Any(a => a.UserId == userId);
        if (!exists)
        {
            _db.InventoryAccesses.Add(new InventoryAccess
            {
                InventoryId = inventory.Id,
                UserId = userId
            });
            await _db.SaveChangesAsync();
        }

        return Json(new { success = true });
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> RemoveAccess(int inventoryId, string userId)
    {
        var inventory = await _db.Inventories
            .Include(i => i.AccessList)
            .FirstOrDefaultAsync(i => i.Id == inventoryId);
        if (inventory == null) return NotFound();

        var currentUserId = CurrentUserId;
        if (!CanManageInventory(inventory, currentUserId)) return Forbid();

        var entries = inventory.AccessList
            .Where(a => a.UserId == userId)
            .ToList();

        if (entries.Count > 0)
        {
            _db.InventoryAccesses.RemoveRange(entries);
            await _db.SaveChangesAsync();
        }

        return Json(new { success = true });
    }

    // ----- Tags -----

    [HttpGet]
    public async Task<IActionResult> SearchTags(string term)
    {
        term = term?.Trim() ?? string.Empty;
        if (term.Length < 1)
            return Json(Array.Empty<object>());

        var tags = await _db.Tags
            .Where(t => EF.Functions.ILike(t.Name, $"{term}%"))
            .OrderBy(t => t.Name)
            .Take(10)
            .Select(t => new { id = t.Id, name = t.Name })
            .ToListAsync();

        return Json(tags);
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> SaveTags([FromBody] SaveTagsDto dto)
    {
        var inventory = await _db.Inventories
            .Include(i => i.Tags)
            .FirstOrDefaultAsync(i => i.Id == dto.InventoryId);
        if (inventory == null) return NotFound();

        var userId = CurrentUserId;
        if (!CanManageInventory(inventory, userId)) return Forbid();

        if (inventory.Version != dto.Version)
            return BadRequest("Conflict: inventory was modified by someone else");

        var names = (dto.Tags ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => t.ToLowerInvariant())
            .Distinct()
            .Take(20)
            .ToList();

        inventory.Tags.Clear();
        if (names.Count > 0)
        {
            var existing = await _db.Tags
                .Where(t => names.Contains(t.Name.ToLower()))
                .ToListAsync();

            foreach (var name in names)
            {
                var tag = existing.FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
                if (tag == null)
                {
                    tag = new Tag { Name = name };
                    _db.Tags.Add(tag);
                }
                inventory.Tags.Add(tag);
            }
        }

        inventory.UpdatedAt = DateTime.UtcNow;
        inventory.Version++;
        await _db.SaveChangesAsync();

        return Json(new { success = true, version = inventory.Version });
    }

    // ----- Discussion & comments -----

    [HttpGet]
    public async Task<IActionResult> Comments(int inventoryId)
    {
        var comments = await _db.Comments
            .Where(c => c.InventoryId == inventoryId)
            .OrderBy(c => c.CreatedAt)
            .Include(c => c.Author)
            .Select(c => new
            {
                id = c.Id,
                content = c.Content,
                createdAt = c.CreatedAt,
                authorName = c.Author!.DisplayName ?? c.Author.UserName ?? c.Author.Email,
                authorId = c.AuthorId
            })
            .ToListAsync();

        return Json(comments);
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> AddComment([FromBody] AddCommentDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Content))
            return BadRequest("Empty content");

        var inventory = await _db.Inventories.FindAsync(dto.InventoryId);
        if (inventory == null) return NotFound();

        var userId = CurrentUserId;
        if (string.IsNullOrEmpty(userId))
            return Forbid();

        var comment = new Comment
        {
            InventoryId = dto.InventoryId,
            AuthorId = userId,
            Content = dto.Content.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();

        return Json(new { success = true });
    }

    public class InventoryFieldsDto
    {
        public int Id { get; set; }
        public int Version { get; set; }
        public string? CustomString1Name { get; set; }
        public bool CustomString1State { get; set; }
        public string? CustomInt1Name { get; set; }
        public bool CustomInt1State { get; set; }
        public string? CustomBool1Name { get; set; }
        public bool CustomBool1State { get; set; }
    }

    public class SaveSettingsDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public bool IsPublic { get; set; }
        public int Version { get; set; }
    }

    public class AddCommentDto
    {
        public int InventoryId { get; set; }
        public string Content { get; set; } = "";
    }

    public class SaveTagsDto
    {
        public int InventoryId { get; set; }
        public int Version { get; set; }
        public string? Tags { get; set; }
    }
}

