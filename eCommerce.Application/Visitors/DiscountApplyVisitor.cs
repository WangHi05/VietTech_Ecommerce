using System;
using eCommerce.Application.Composites;

namespace eCommerce.Application.Visitors
{
    public class DiscountApplyVisitor : ICatalogVisitor
    {
        private readonly decimal _discountPercent;
        private readonly string _targetCategoryName;
        private int _categoryDepth = 0;
        private int _targetDepth = -1;
        private const string DiscountMarker = "DISCOUNT_APPLIED";

        public DiscountApplyVisitor(decimal discountPercent, string targetCategoryName)
        {
            if (discountPercent < 0 || discountPercent > 1)
                throw new ArgumentException("Tỷ lệ giảm giá phải nằm trong khoảng 0 - 1.", nameof(discountPercent));

            _discountPercent = discountPercent;
            _targetCategoryName = targetCategoryName;
        }

        public void VisitCategory(CategoryComposite category)
        {
            if (category.Name == _targetCategoryName && _targetDepth == -1)
            {
                _targetDepth = _categoryDepth;
                Console.WriteLine($"[Bắt đầu áp dụng giảm giá {_discountPercent * 100}% trong danh mục: {category.Name}]");
            }

            _categoryDepth++;
        }

        public void VisitProduct(ProductLeaf product)
        {
            // Chỉ áp dụng giảm giá khi đang bên trong danh mục mục tiêu
            if (_targetDepth >= 0 && _categoryDepth == _targetDepth + 1)
            {
                if (!product.Name.Contains(DiscountMarker))
                {
                    decimal originalPrice = product.Price;
                    decimal discountAmount = originalPrice * _discountPercent;
                    product.Price = originalPrice - discountAmount;

                    Console.WriteLine($"  ✓ {product.Name}: {originalPrice:N0}đ → {product.Price:N0}đ (-{discountAmount:N0}đ)");
                }
            }
        }

        public void EndTargetCategory()
        {
            if (_targetDepth >= 0)
            {
                Console.WriteLine($"[Kết thúc áp dụng giảm giá]");
                Console.WriteLine();
                _targetDepth = -1;
            }
        }
    }
}
