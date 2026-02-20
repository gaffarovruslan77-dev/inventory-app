namespace InventoryApp.Web.Models;

public class InventoryAccess
{
    public int Id { get; set; }
    public int InventoryId { get; set; }
    public Inventory? Inventory { get; set; }
    public string UserId { get; set; } = "";
    public AppUser? User { get; set; }
}
