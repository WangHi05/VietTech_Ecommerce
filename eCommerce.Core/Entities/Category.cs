using System.Collections.Generic;

namespace eCommerce.Core.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Navigation property: Một Category có nhiều Product
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}