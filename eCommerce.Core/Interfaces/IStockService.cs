using System.Collections.Generic;
using System.Threading.Tasks;
using eCommerce.Core.Entities;

namespace eCommerce.Core.Interfaces
{
    public interface IStockService
    {
        /// <summary>
        /// Kiểm tra xem sản phẩm có đủ số lượng trong kho hay không
        /// </summary>
        Task<bool> CheckStockAvailability(int productId, int quantity);

        /// <summary>
        /// Trừ số lượng tồn kho khi đặt hàng
        /// </summary>
        Task<bool> DeductStock(int productId, int quantity, int orderId, string userName = "");

        /// <summary>
        /// Hoàn lại số lượng tồn kho khi hủy đơn hoặc trả hàng
        /// </summary>
        Task<bool> RestoreStock(int productId, int quantity, int orderId, string reason, string userName = "");

        /// <summary>
        /// Nhập hàng mới vào kho (Admin)
        /// </summary>
        Task<bool> ImportStock(int productId, int quantity, string reason, string userName);

        /// <summary>
        /// Điều chỉnh tồn kho (Admin) - dùng khi kiểm kê, sản phẩm hỏng, mất...
        /// </summary>
        Task<bool> AdjustStock(int productId, int newQuantity, string reason, string userName);

        /// <summary>
        /// Lấy danh sách sản phẩm sắp hết hàng (dưới mức MinStockLevel)
        /// </summary>
        Task<List<Product>> GetLowStockProducts();

        /// <summary>
        /// Lấy lịch sử xuất nhập kho của một sản phẩm
        /// </summary>
        Task<List<StockHistory>> GetStockHistory(int productId);

        /// <summary>
        /// Lấy tất cả lịch sử xuất nhập kho (Admin)
        /// </summary>
        Task<List<StockHistory>> GetAllStockHistory();
    }
}
