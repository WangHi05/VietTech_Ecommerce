namespace eCommerce.Application.DesignPatterns.Iterator
{
    /// Iterator interface - định nghĩa các phương thức duyệt collection
    public interface IIterator<T>
    {
        T? Current { get; }
        bool MoveNext();
        void Reset();
    }
}
