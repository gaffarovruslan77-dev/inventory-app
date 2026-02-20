namespace InventoryApp.Web.Models;

public class Like
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public Item? Item { get; set; }
    public string UserId { get; set; } = "";
    public AppUser? User { get; set; }
}
