namespace eCommerce.Application.DesignPatterns.Iterator
{
    /// <summary>
    /// Iterator interface - định nghĩa các phương thức duyệt collection
    /// </summary>
    public interface IIterator<T>
    {
        T? Current { get; }
        bool MoveNext();
        void Reset();
    }
}
