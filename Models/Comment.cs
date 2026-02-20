namespace InventoryApp.Web.Models;

public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int InventoryId { get; set; }
    public Inventory? Inventory { get; set; }
    public string AuthorId { get; set; } = "";
    public AppUser? Author { get; set; }
}
