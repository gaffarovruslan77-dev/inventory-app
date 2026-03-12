using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryApp.Web.Data;
using InventoryApp.Web.Models;

namespace InventoryApp.Web.Controllers;

public class LikeController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;

    public LikeController(AppDbContext db, UserManager<AppUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Count(int itemId)
    {
        var count = await _db.Likes.CountAsync(l => l.ItemId == itemId);
        return Json(new { count });
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> Toggle([FromBody] ToggleLikeDto dto)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Forbid();

        var itemExists = await _db.Items.AnyAsync(i => i.Id == dto.ItemId);
        if (!itemExists) return NotFound();

        var existing = await _db.Likes
            .FirstOrDefaultAsync(l => l.ItemId == dto.ItemId && l.UserId == userId);

        if (existing == null)
        {
            _db.Likes.Add(new Like { ItemId = dto.ItemId, UserId = userId });
        }
        else
        {
            _db.Likes.Remove(existing);
        }

        await _db.SaveChangesAsync();

        var count = await _db.Likes.CountAsync(l => l.ItemId == dto.ItemId);
        var liked = existing == null;
        return Json(new { liked, count });
    }

    public class ToggleLikeDto
    {
        public int ItemId { get; set; }
    }
}

