using System;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using Stripe.Checkout.Core.Services;
using Stripe.Checkout.Managers;
using Stripe.Checkout.Services;
using VirtoCommerce.Domain.Payment.Services;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Settings;

namespace Stripe.Checkout
{
    public class Module : ModuleBase
    {
        private readonly IUnityContainer _container;

        public Module(IUnityContainer container)
        {
            _container = container;
        }

        public override void Initialize()
        {
            _container.RegisterType<IStripeCheckoutService, StripeCheckoutService>();

            var settingsManager = ServiceLocator.Current.GetInstance<ISettingsManager>();

            Func<StripeCheckoutPaymentMethod> stripePaymentMethogd = () =>
            {
                var paymentMethod = new StripeCheckoutPaymentMethod(_container.Resolve<IStripeCheckoutService>());
                paymentMethod.Name = "Stripe Checkout Gateway";
                paymentMethod.Description = "Stripe Checkout payment gateway integration";
                paymentMethod.LogoUrl = "";
                paymentMethod.Settings = settingsManager.GetModuleSettings("Stripe.Checkout");
                return paymentMethod;
            };

            var paymentMethodsService = _container.Resolve<IPaymentMethodsService>();
            paymentMethodsService.RegisterPaymentMethod(stripePaymentMethogd);
        }
    }
}
