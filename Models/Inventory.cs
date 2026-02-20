namespace InventoryApp.Web.Models;

public class Inventory
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsPublic { get; set; } = false;
    public int Version { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string CreatorId { get; set; } = "";
    public AppUser? Creator { get; set; }
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
    public string? CustomString1Name { get; set; }
    public bool CustomString1State { get; set; }
    public string? CustomString2Name { get; set; }
    public bool CustomString2State { get; set; }
    public string? CustomString3Name { get; set; }
    public bool CustomString3State { get; set; }
    public string? CustomInt1Name { get; set; }
    public bool CustomInt1State { get; set; }
    public string? CustomInt2Name { get; set; }
    public bool CustomInt2State { get; set; }
    public string? CustomInt3Name { get; set; }
    public bool CustomInt3State { get; set; }
    public string? CustomBool1Name { get; set; }
    public bool CustomBool1State { get; set; }
    public string? CustomBool2Name { get; set; }
    public bool CustomBool2State { get; set; }
    public string? CustomBool3Name { get; set; }
    public bool CustomBool3State { get; set; }
    public string? CustomText1Name { get; set; }
    public bool CustomText1State { get; set; }
    public string? CustomText2Name { get; set; }
    public bool CustomText2State { get; set; }
    public string? CustomText3Name { get; set; }
    public bool CustomText3State { get; set; }
    public string? CustomLink1Name { get; set; }
    public bool CustomLink1State { get; set; }
    public string? CustomLink2Name { get; set; }
    public bool CustomLink2State { get; set; }
    public string? CustomLink3Name { get; set; }
    public bool CustomLink3State { get; set; }
    public ICollection<Item> Items { get; set; } = new List<Item>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public ICollection<InventoryAccess> AccessList { get; set; } = new List<InventoryAccess>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
