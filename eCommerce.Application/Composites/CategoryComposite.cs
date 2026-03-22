using System;
using System.Collections.Generic;
using System.Linq;
using eCommerce.Application.Visitors;

namespace eCommerce.Application.Composites
{
    public class CategoryComposite : ICatalogComponent
    {
        // Danh sách này có thể chứa cả ProductLeaf lẫn CategoryComposite
        private readonly List<ICatalogComponent> _children = new();
        
        public string Name { get; set; }

        public CategoryComposite(string name)
        {
            Name = name;
        }

        // Các hàm để quản lý cây
        public void Add(ICatalogComponent component) => _children.Add(component);
        public void Remove(ICatalogComponent component) => _children.Remove(component);

        // tự động gọi đệ quy xuống tất cả các nhánh con để cộng dồn tồn kho
        public int GetTotalStock()
        {
            return _children.Sum(child => child.GetTotalStock());
        }

        public string GenerateHtmlTree()
        {
            // Dùng <ul> để tạo danh sách lồng nhau
            var html = $"<li><b>[Danh mục] {Name}</b> (Tổng tồn kho: {GetTotalStock()})<ul>";
            
            foreach (var child in _children)
            {
                html += child.GenerateHtmlTree(); // Đệ quy: Tự động lồng các thẻ <li> con vào trong <ul>
            }
            
            html += "</ul></li>";
            return html;
        }

        public void Accept(ICatalogVisitor visitor)
        {
            visitor.VisitCategory(this);
            // Đệ quy: chuyển visitor xuống tất cả component con
            foreach (var child in _children)
            {
                child.Accept(visitor);
            }
        }
    }
}