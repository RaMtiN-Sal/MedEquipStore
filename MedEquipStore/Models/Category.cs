namespace MedEquipStore.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public required string Name { get;set;}
        public required string Description { get; set; }
        public string? ImageUrl { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>(); 

    }
}
