using System;
using eCommerce.Application.Composites;

namespace eCommerce.Application.Visitors
{
    public class CatalogVisitorDemo
    {
        public static void RunDemo()
        {
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║           DEMO VISITOR PATTERN - CATALOG SYSTEM              ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");

            Console.WriteLine("📦 BƯỚC 1: Xây dựng cây danh mục sản phẩm");
            Console.WriteLine(new string('=', 70));
            var catalog = BuildCatalog();
            Console.WriteLine(catalog.GenerateHtmlTree());
            Console.WriteLine("\n");

            Console.WriteLine("\n📊 BƯỚC 2: Báo cáo giá trị tồn kho");
            Console.WriteLine(new string('=', 70));
            var priceVisitor = new PriceReportVisitor();
            catalog.Accept(priceVisitor);
            Console.WriteLine(priceVisitor.GenerateReport());

            Console.WriteLine("\n💰 BƯỚC 3: Áp dụng giảm giá (10% cho danh mục 'Điện thoại')");
            Console.WriteLine(new string('=', 70));
            var discountVisitor = new DiscountApplyVisitor(0.1m, "Điện thoại");
            catalog.Accept(discountVisitor);
            discountVisitor.EndTargetCategory();

            Console.WriteLine("\n🔍 BƯỚC 4: Sinh metadata SEO");
            Console.WriteLine(new string('=', 70));
            var seoVisitor = new SeoMetadataVisitor();
            catalog.Accept(seoVisitor);
            Console.WriteLine(seoVisitor.GenerateReport());

            Console.WriteLine("\n✅ Demo hoàn tất!");
        }

        private static CategoryComposite BuildCatalog()
        {
            var root = new CategoryComposite("eCommerce");

            var phones = new CategoryComposite("Điện thoại");
            phones.Add(new ProductLeaf("iPhone 15 Pro", 25, 29990000, "Apple"));
            phones.Add(new ProductLeaf("Samsung Galaxy S24", 30, 24990000, "Samsung"));
            phones.Add(new ProductLeaf("Xiaomi 14", 40, 12990000, "Xiaomi"));
            root.Add(phones);

            var accessories = new CategoryComposite("Phụ kiện");
            accessories.Add(new ProductLeaf("Tai nghe Bluetooth", 50, 1500000, "Sony"));
            accessories.Add(new ProductLeaf("Ốp lưng", 100, 250000, "Spigen"));
            root.Add(accessories);

            var laptops = new CategoryComposite("Laptop");
            laptops.Add(new ProductLeaf("MacBook Pro 16", 10, 64990000, "Apple"));
            laptops.Add(new ProductLeaf("Dell XPS 13", 15, 32990000, "Dell"));
            root.Add(laptops);

            Console.WriteLine($"✓ Tạo {3} danh mục với {8} sản phẩm");
            Console.WriteLine();

            return root;
        }

        private class PriceCheckVisitor : ICatalogVisitor
        {
            public void VisitCategory(CategoryComposite category) { }

            public void VisitProduct(ProductLeaf product)
            {
                Console.WriteLine($"  - {product.Name}: {product.Price:N0}đ");
            }
        }
    }
}
