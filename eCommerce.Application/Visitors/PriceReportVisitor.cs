using System;
using System.Collections.Generic;
using System.Linq;
using eCommerce.Application.Composites;

namespace eCommerce.Application.Visitors
{
    public class PriceReportVisitor : ICatalogVisitor
    {
        private readonly Dictionary<string, decimal> _categoryReport = new();
        private decimal _totalValue = 0;

        public void VisitCategory(CategoryComposite category)
        {
            if (!_categoryReport.ContainsKey(category.Name))
                _categoryReport[category.Name] = 0;
        }

        public void VisitProduct(ProductLeaf product)
        {
            decimal productValue = product.Price * product.Stock;
            _totalValue += productValue;
            Console.WriteLine($"  └─ {product.Name}: {product.Price:N0}đ × {product.Stock} = {productValue:N0}đ");
        }

        public string GenerateReport()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("╔════════════════════════════════════════════════════════════════╗");
            report.AppendLine("║           BÁO CÁO GIÁ TRỊ TỒN KHO THEO DANH MỤC              ║");
            report.AppendLine("╚════════════════════════════════════════════════════════════════╝");
            report.AppendLine();

            if (_categoryReport.Count == 0)
            {
                report.AppendLine("Không có dữ liệu báo cáo.");
                return report.ToString();
            }

            var sortedCategories = _categoryReport.OrderByDescending(x => x.Value).ToList();
            int rank = 1;

            foreach (var category in sortedCategories)
            {
                report.AppendLine($"{rank}. {category.Key}:");
                report.AppendLine($"   Giá trị: {category.Value:N0} VND");
                report.AppendLine();
                rank++;
            }

            report.AppendLine("────────────────────────────────────────────────────────────────");
            report.AppendLine($"TỔNG GIÁ TRỊ TỒN KHO: {_totalValue:N0} VND");
            report.AppendLine("────────────────────────────────────────────────────────────────");

            return report.ToString();
        }

        public decimal GetTotalValue() => _totalValue;
    }
}
