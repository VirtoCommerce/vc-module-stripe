using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Domain.Store.Model;

namespace Stripe.Checkout.Core.Model
{
    public class StripeCheckoutSettings
    {
        public string PublishableKey { get; set; }

        public string TokenAttributeName { get; set; }

        public Store Store { get; set; }

        public CustomerOrder Order { get; set; }
    }
}