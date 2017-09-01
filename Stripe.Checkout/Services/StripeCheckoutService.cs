using System;
using System.IO;
using System.Reflection;
using DotLiquid;
using Stripe.Checkout.Core.Model;
using Stripe.Checkout.Core.Services;
using VirtoCommerce.Domain.Store.Model;

namespace Stripe.Checkout.Services
{
    [Obsolete]
    public class StripeCheckoutService : IStripeCheckoutService
    {
        /// <summary>
        /// Returns Stripe Checkout Card Payment Form built with Stripe Elements and Stripe.js
        /// </summary>
        public string GetCheckoutFormContent(StripeCheckoutSettings context)
        {
            return GetForm(context);
        }

        private string GetForm(StripeCheckoutSettings settings)
        {
            var assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream("Stripe.Checkout.Content.paymentForm.liquid");
            if (stream == null)
            {
                return string.Empty;
            }

            StreamReader sr = new StreamReader(stream);
            var formContent = sr.ReadToEnd();

            Template template = Template.Parse(formContent);
            var content = template.Render(Hash.FromAnonymousObject(new
            {
                publishableKey = settings.PublishableKey,
                storeUrl = GetStoreUrl(settings.Store),
                stripeTokenAttrName = settings.TokenAttributeName,
                orderId = settings.Order.Number
            }));

            return content;
        }

        private string GetStoreUrl(Store store)
        {
            if (!string.IsNullOrEmpty(store.SecureUrl))
            {
                return store.SecureUrl;
            }

            if (!string.IsNullOrEmpty(store.Url))
            {
                return store.Url;
            }

            return "";
        }
    }
}