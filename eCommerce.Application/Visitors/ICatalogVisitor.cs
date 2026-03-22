namespace eCommerce.Application.Visitors
{
    public interface ICatalogVisitor
    {
        void VisitCategory(Composites.CategoryComposite category);
        void VisitProduct(Composites.ProductLeaf product);
    }
}
