using Stripe.Checkout.Core.Model;

namespace Stripe.Checkout.Core.Services
{
    public interface IStripeCheckoutService
    {
        string GetCheckoutFormContent(StripeCheckoutSettings context);
    }
}