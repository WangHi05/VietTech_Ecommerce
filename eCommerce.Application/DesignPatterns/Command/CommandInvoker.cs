namespace eCommerce.Application.DesignPatterns.Command;

// CommandInvoker: Lớp gọi/thực thi các command
// Tách biệt sender (người gửi lệnh) khỏi receiver (người thực thi lệnh)
// Dễ dàng mở rộng thêm chức năng: queue command, undo, redo
public class CommandInvoker
{
    // Phương thức chạy một command bất kỳ
    public void RunCommand(ICommand command)
    {
        command.Execute();
    }

    // Thực thi nhiều command theo thứ tự
    public void RunCommands(IEnumerable<ICommand> commands)
    {
        foreach (var command in commands)
        {
            command.Execute();
        }
    }
}
