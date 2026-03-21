namespace eCommerce.Application.Mediators
{
    public interface ICheckoutMediator
    {
        void Notify(CheckoutComponent sender, string eventCode);
    }

    public abstract class CheckoutComponent
    {
        protected ICheckoutMediator? _mediator;

        public void SetMediator(ICheckoutMediator mediator)
        {
            _mediator = mediator;
        }
    }
}