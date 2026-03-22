using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using eCommerce.Application.Composites;

namespace eCommerce.Application.Visitors
{
    public class SeoMetadataVisitor : ICatalogVisitor
    {
        private readonly Dictionary<string, SeoMetadata> _seoMetadata = new();

        public void VisitCategory(CategoryComposite category)
        {
            string slug = GenerateSlug(category.Name);
            string metaTitle = $"{category.Name} - eCommerce Store";
            string metaDescription = $"Danh mục {category.Name}: Khám phá các sản phẩm chất lượng cao trong danh mục này.";

            var metadata = new SeoMetadata(slug, metaTitle, metaDescription);
            _seoMetadata[category.Name] = metadata;

            Console.WriteLine($"[Danh mục] {category.Name}");
            Console.WriteLine($"  ├─ Slug: {slug}");
            Console.WriteLine($"  ├─ Title: {metaTitle}");
            Console.WriteLine($"  └─ Description: {metaDescription}");
            Console.WriteLine();
        }

        public void VisitProduct(ProductLeaf product)
        {
            string slug = GenerateSlug($"{product.Brand} {product.Name}".Trim());
            string metaTitle = $"{product.Name} - {product.Brand}";
            string metaDescription = $"Mua {product.Name} từ {product.Brand}. Giá: {product.Price:N0}đ. Tồn kho: {product.Stock}";

            var metadata = new SeoMetadata(slug, metaTitle, metaDescription);
            _seoMetadata[product.Name] = metadata;

            Console.WriteLine($"[Sản phẩm] {product.Name}");
            Console.WriteLine($"  ├─ Slug: {slug}");
            Console.WriteLine($"  ├─ Title: {metaTitle}");
            Console.WriteLine($"  └─ Description: {metaDescription}");
            Console.WriteLine();
        }

        private static string GenerateSlug(string text)
        {
            text = text.ToLower();
            text = Regex.Replace(text, @"[^a-z0-9\s-]", "");
            text = Regex.Replace(text, @"\s+", "-");
            text = Regex.Replace(text, @"-+", "-");
            return text.Trim('-');
        }

        public Dictionary<string, SeoMetadata> GetSeoMetadata() => _seoMetadata;

        public string GenerateReport()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("╔════════════════════════════════════════════════════════════════╗");
            report.AppendLine("║                  BÁO CÁO METADATA SEO                          ║");
            report.AppendLine("╚════════════════════════════════════════════════════════════════╝");
            report.AppendLine();

            if (_seoMetadata.Count == 0)
            {
                report.AppendLine("Không có metadata SEO được sinh ra.");
                return report.ToString();
            }

            int index = 1;
            foreach (var entry in _seoMetadata)
            {
                report.AppendLine($"{index}. {entry.Key}:");
                report.AppendLine($"   Slug: {entry.Value.Slug}");
                report.AppendLine($"   MetaTitle: {entry.Value.MetaTitle}");
                report.AppendLine($"   MetaDescription: {entry.Value.MetaDescription}");
                report.AppendLine();
                index++;
            }

            report.AppendLine("────────────────────────────────────────────────────────────────");
            report.AppendLine($"Tổng số entity: {_seoMetadata.Count}");
            report.AppendLine("────────────────────────────────────────────────────────────────");

            return report.ToString();
        }
    }
}
