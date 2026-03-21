namespace eCommerce.Application.DesignPatterns.Singleton;

// Singleton Pattern: Đảm bảo chỉ có một instance duy nhất của logger.
// Dùng Lazy<T> đảm bảo thread-safe lazy initialization.
// Áp dụng cho: Log sự kiện tạo đơn hàng, cập nhật sản phẩm, thanh toán, lỗi hệ thống.
public sealed class AppLoggerSingleton
{
    private static readonly Lazy<AppLoggerSingleton> _instance = new(() => new AppLoggerSingleton());

    public static AppLoggerSingleton Instance => _instance.Value;

    private AppLoggerSingleton()
    {
        Console.WriteLine("[AppLogger] Khởi tạo Singleton Logger");
    }

    public void Log(string message)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        Console.WriteLine($"[{timestamp}] LOG: {message}");
    }

    public void LogError(string message)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[{timestamp}] ERROR: {message}");
        Console.ResetColor();
    }

    public void LogWarning(string message)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[{timestamp}] WARNING: {message}");
        Console.ResetColor();
    }
}
