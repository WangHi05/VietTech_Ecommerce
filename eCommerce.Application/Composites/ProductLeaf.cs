using System;
using eCommerce.Application.Visitors;

namespace eCommerce.Application.Composites
{
    public class ProductLeaf : ICatalogComponent
    {
        public string Name { get; set; }
        public int Stock { get; set; }
        public decimal Price { get; set; }
        public string Brand { get; set; } = string.Empty;

        public ProductLeaf(string name, int stock, decimal price = 0, string brand = "")
        {
            Name = name;
            Stock = stock;
            Price = price;
            Brand = brand;
        }

        // Với một sản phẩm đơn lẻ, tổng tồn kho chính là tồn kho của nó
        public int GetTotalStock() => Stock;

        public string GenerateHtmlTree()
        {
            // Dùng thẻ <li> để đại diện cho 1 dòng sản phẩm
            return $"<li>Sản phẩm: <span class='text-primary'>{Name}</span> - Giá: {Price:N0}đ - Tồn kho: <b>{Stock}</b></li>";
        }

        public void Accept(ICatalogVisitor visitor)
        {
            visitor.VisitProduct(this);
        }
    }   
}