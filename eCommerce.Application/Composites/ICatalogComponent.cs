using eCommerce.Application.Visitors;

namespace eCommerce.Application.Composites
{
    public interface ICatalogComponent
    {
        string Name { get; }
        int GetTotalStock(); 
        string GenerateHtmlTree();
        void Accept(ICatalogVisitor visitor);
    }
}