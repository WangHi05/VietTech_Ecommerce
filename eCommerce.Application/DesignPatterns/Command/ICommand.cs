namespace eCommerce.Application.DesignPatterns.Command;

// Command Pattern: Interface định nghĩa hành động (command) có thể được execute
// Dùng để đóng gói yêu cầu và tham số hóa các đối tượng với các hoạt động
public interface ICommand
{
    void Execute();
}
