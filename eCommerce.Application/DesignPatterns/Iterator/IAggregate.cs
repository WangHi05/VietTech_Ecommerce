namespace eCommerce.Application.DesignPatterns.Iterator
{
    /// <summary>
    /// Aggregate interface - tạo iterator để duyệt collection
    /// </summary>
    public interface IAggregate<T>
    {
        IIterator<T> CreateIterator();
    }
}
