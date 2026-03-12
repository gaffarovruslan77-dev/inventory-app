using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryApp.Web.Data;

namespace InventoryApp.Web.Controllers;

public class SearchController : Controller
{
    private readonly AppDbContext _db;

    public SearchController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string q)
    {
        q = (q ?? "").Trim();
        if (q.Length == 0)
            return View(new SearchViewModel { Query = q });

        // PostgreSQL full-text search via raw SQL (Inventories title+description)
        var inventories = await _db.Inventories
            .FromSqlInterpolated($@"
                SELECT *
                FROM ""Inventories""
                WHERE to_tsvector('simple', coalesce(""Title"", '') || ' ' || coalesce(""Description"", ''))
                      @@ plainto_tsquery('simple', {q})
                ORDER BY ""CreatedAt"" DESC
                LIMIT 50
            ")
            .ToListAsync();

        var invIds = inventories.Select(i => i.Id).ToList();

        // Items search (match text OR belong to matched inventories)
        var matchedItems = await _db.Items
            .FromSqlInterpolated($@"
                SELECT *
                FROM ""Items""
                WHERE to_tsvector('simple',
                        coalesce(""CustomId"", '') || ' ' ||
                        coalesce(""CustomString1"", '') || ' ' ||
                        coalesce(""CustomString2"", '') || ' ' ||
                        coalesce(""CustomString3"", '') || ' ' ||
                        coalesce(""CustomText1"", '') || ' ' ||
                        coalesce(""CustomText2"", '') || ' ' ||
                        coalesce(""CustomText3"", '')
                      )
                      @@ plainto_tsquery('simple', {q})
                ORDER BY ""CreatedAt"" DESC
                LIMIT 50
            ")
            .ToListAsync();

        var invItems = invIds.Count == 0
            ? new List<Models.Item>()
            : await _db.Items
                .Where(i => invIds.Contains(i.InventoryId))
                .OrderByDescending(i => i.CreatedAt)
                .Take(200)
                .ToListAsync();

        var items = matchedItems
            .Concat(invItems)
            .GroupBy(i => i.Id)
            .Select(g => g.First())
            .ToList();

        // load inventory for display
        var itemIds = items.Select(i => i.Id).ToList();
        items = await _db.Items
            .Where(i => itemIds.Contains(i.Id))
            .Include(i => i.Inventory)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return View(new SearchViewModel
        {
            Query = q,
            Inventories = inventories,
            Items = items
        });
    }

    public class SearchViewModel
    {
        public string Query { get; set; } = "";
        public List<Models.Inventory> Inventories { get; set; } = new();
        public List<Models.Item> Items { get; set; } = new();
    }
}

