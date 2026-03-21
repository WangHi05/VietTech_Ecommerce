namespace eCommerce.Application.Flyweights
{
    public class BrandFlyweight
    {
        public string BrandName { get; }
        public string LogoUrl { get; }
        public string WarrantyPolicy { get; }

        public BrandFlyweight(string brandName, string logoUrl, string warrantyPolicy)
        {
            BrandName = brandName;
            LogoUrl = logoUrl;
            WarrantyPolicy = warrantyPolicy;
        }

        //nhận vào dữ liệu riêng (Extrinsic) từ bên ngoài truyền vào
        public void DisplayProductInfo(string productName, decimal price)
        {
            Console.WriteLine($"[{BrandName}] {productName} - Giá: {price:N0}đ");
            Console.WriteLine($"Chính sách: {WarrantyPolicy}");
        }
    }
}