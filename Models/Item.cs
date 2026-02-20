namespace InventoryApp.Web.Models;

public class Item
{
    public int Id { get; set; }
    public string? CustomId { get; set; }
    public int Version { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int InventoryId { get; set; }
    public Inventory? Inventory { get; set; }
    public string CreatedById { get; set; } = "";
    public AppUser? CreatedBy { get; set; }
    public string? CustomString1 { get; set; }
    public string? CustomString2 { get; set; }
    public string? CustomString3 { get; set; }
    public int? CustomInt1 { get; set; }
    public int? CustomInt2 { get; set; }
    public int? CustomInt3 { get; set; }
    public bool? CustomBool1 { get; set; }
    public bool? CustomBool2 { get; set; }
    public bool? CustomBool3 { get; set; }
    public string? CustomText1 { get; set; }
    public string? CustomText2 { get; set; }
    public string? CustomText3 { get; set; }
    public string? CustomLink1 { get; set; }
    public string? CustomLink2 { get; set; }
    public string? CustomLink3 { get; set; }
    public ICollection<Like> Likes { get; set; } = new List<Like>();
}
