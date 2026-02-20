using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using InventoryApp.Web.Models;

namespace InventoryApp.Web.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<InventoryAccess> InventoryAccesses => Set<InventoryAccess>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Like> Likes => Set<Like>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Like>()
            .HasIndex(l => new { l.ItemId, l.UserId })
            .IsUnique();

        builder.Entity<InventoryAccess>()
            .HasIndex(a => new { a.InventoryId, a.UserId })
            .IsUnique();
    }
}
