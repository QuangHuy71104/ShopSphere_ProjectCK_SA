namespace ShopSphere.Domain.Entities;

using System.Text.Json.Serialization;

public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;


    public Guid? ParentId { get; set; }
    [JsonIgnore]
    public Category? Parent { get; set; }
    [JsonIgnore]
    public ICollection<Category> Children { get; set; } = new List<Category>();


    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
