using System;
using System.Collections.Generic;

namespace eCommerce.Application.Flyweights
{
    public class BrandFlyweightFactory
    {
        // Dictionary dùng làm Cache lưu trữ các Flyweight
        private readonly Dictionary<string, BrandFlyweight> _flyweights = new();

        public BrandFlyweight GetBrandFlyweight(string brandName, string logoUrl, string warrantyPolicy)
        {
            // Dùng tên thương hiệu làm Key (chuyển về chữ thường để tránh phân biệt hoa/thường)
            string key = brandName.ToLower();

            if (!_flyweights.ContainsKey(key))
            {
                // Nếu chưa có trong Cache thì tạo mới và lưu lại
                Console.WriteLine($"[FlyweightFactory] Đang tạo mới bộ nhớ đệm cho thương hiệu: {brandName}");
                _flyweights[key] = new BrandFlyweight(brandName, logoUrl, warrantyPolicy);
            }
            else
            {
                Console.WriteLine($"[FlyweightFactory] Sử dụng lại bộ nhớ đệm của thương hiệu: {brandName}");
            }

            return _flyweights[key];
        }

        public int GetCacheSize() => _flyweights.Count;
    }
}