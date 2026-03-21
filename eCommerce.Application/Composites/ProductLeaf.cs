using System;

namespace eCommerce.Application.Composites
{
    public class ProductLeaf : ICatalogComponent
    {
        public string Name { get; set; }
        public int Stock { get; set; }

        public ProductLeaf(string name, int stock)
        {
            Name = name;
            Stock = stock;
        }

        // Với một sản phẩm đơn lẻ, tổng tồn kho chính là tồn kho của nó
        public int GetTotalStock() => Stock;

        public string GenerateHtmlTree()
        {
            // Dùng thẻ <li> để đại diện cho 1 dòng sản phẩm
            return $"<li>Sản phẩm: <span class='text-primary'>{Name}</span> - Tồn kho: <b>{Stock}</b></li>";
        }
    }   
}