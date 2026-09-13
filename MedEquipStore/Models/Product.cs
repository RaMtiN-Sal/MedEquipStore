using System.ComponentModel.DataAnnotations;

namespace MedEquipStore.Models
{
    public class Product
    {
        
        public int ProductId { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public decimal Price { get; set; }
        public required string PartNumber { get; set; }
        public int StockQuantity { get; set; }
        public required string Manufacturer { get; set; }
        public string? ImageUrl { get; set; }
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }


    }
}
