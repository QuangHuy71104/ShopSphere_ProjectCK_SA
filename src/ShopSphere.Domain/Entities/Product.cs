namespace ShopSphere.Domain.Entities;

using System.Text.Json.Serialization;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public decimal Price { get; set; }
    public int Stock { get; set; }

    public Guid CategoryId { get; set; }
    [JsonIgnore]
    public Category Category { get; set; } = default!;  


    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
