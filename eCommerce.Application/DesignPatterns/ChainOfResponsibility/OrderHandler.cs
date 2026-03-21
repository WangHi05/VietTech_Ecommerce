namespace eCommerce.Application.DesignPatterns.ChainOfResponsibility;

using eCommerce.Core.Entities;

// OrderHandler: Abstract base class
// Chain of Responsibility Pattern: Tạo chuỗi các handler để xử lý yêu cầu
// Mỗi handler có thể: xử lý, từ chối, hoặc chuyển sang handler tiếp theo
// Áp dụng: Pipeline validation đơn hàng, authorization, logging
public abstract class OrderHandler
{
    protected OrderHandler? _next;

    public virtual OrderHandler SetNext(OrderHandler? next)
    {
        _next = next;
        return next ?? this;
    }

    public abstract void Handle(Order order);

    protected virtual void PassToNext(Order order)
    {
        _next?.Handle(order);
    }
}
