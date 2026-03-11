namespace eCommerce.Application.DesignPatterns.Iterator
{
    /// Aggregate interface - tạo iterator để duyệt collection
    public interface IAggregate<T>
    {
        IIterator<T> CreateIterator();
    }
}
