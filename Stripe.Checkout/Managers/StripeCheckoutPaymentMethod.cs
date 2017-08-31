using System;
using System.Collections.Specialized;
using Stripe.Checkout.Core.Model;
using Stripe.Checkout.Core.Services;
using Stripe.Checkout.Helpers;
using VirtoCommerce.Domain.Payment.Model;

namespace Stripe.Checkout.Managers
{
    public class StripeCheckoutPaymentMethod : PaymentMethod
    {
        private const string _stripeTokenAttrName = "stripeToken";

        private const string _stripeModeStoreSetting = "Stripe.Checkout.Mode";

        private const string _livesPublishableKeySetting = "Stripe.Checkout.ApiPublishableKeyLive";
        private const string _liveSecretKeySetting = "Stripe.Checkout.ApiSecretKeyLive";

        private const string _testPublishableKeySetting = "Stripe.Checkout.ApiPublishableKeyTest";
        private const string _testSecretKeySetting = "Stripe.Checkout.ApiSecretKeyTest";

        private string ApiMode
        {
            get
            {
                return GetSetting(_stripeModeStoreSetting);
            }
        }

        private string ApiPublishableKey
        {
            get
            {
                if (ApiMode.Equals("test"))
                    return GetSetting(_testPublishableKeySetting);

               return GetSetting(_livesPublishableKeySetting);
            }
        }

        private string ApiSecretKey
        {
            get
            {
                if (ApiMode.Equals("test"))
                    return GetSetting(_testSecretKeySetting);

                return GetSetting(_liveSecretKeySetting);
            }
        }

        public override PaymentMethodType PaymentMethodType
        {
            get { return PaymentMethodType.PreparedForm; }
        }

        public override PaymentMethodGroupType PaymentMethodGroupType
        {
            get { return PaymentMethodGroupType.Alternative; }
        }

        private readonly IStripeCheckoutService _stripeCheckoutService;

        public StripeCheckoutPaymentMethod(IStripeCheckoutService stripeCheckoutService) 
            : base("StripeCheckout")
        {
            _stripeCheckoutService = stripeCheckoutService;
        }

        public override ProcessPaymentResult ProcessPayment(ProcessPaymentEvaluationContext context)
        {
            var result = new ProcessPaymentResult();
            if (context.Order != null && context.Store != null)
            {
                result = PrepareFormContent(context);
            }
            return result;
        }

        private ProcessPaymentResult PrepareFormContent(ProcessPaymentEvaluationContext context)
        {
            var formContent = _stripeCheckoutService.GetCheckoutFormContent(new StripeCheckoutSettings
            {
                PublishableKey = ApiPublishableKey,
                TokenAttributeName = _stripeTokenAttrName,
                Order = context.Order,
                Store = context.Store,
            });

            var result = new ProcessPaymentResult();
            result.IsSuccess = true;
            result.NewPaymentStatus = context.Payment.PaymentStatus = PaymentStatus.Pending;
            result.HtmlForm = formContent;
            result.OuterId = null;

            return result;
        }

        public override PostProcessPaymentResult PostProcessPayment(PostProcessPaymentEvaluationContext context)
        {
            var result = new PostProcessPaymentResult();

            if (context.Parameters == null || !context.Parameters.HasKeys())
            {
                result.ErrorMessage = "NoStripeTokenPresent";
                return result;
            }

            var stripeTokenId = context.Parameters.Get(_stripeTokenAttrName);
            if (stripeTokenId == null)
            {
                result.ErrorMessage = "NoStripeTokenPresent";
                return result;
            }

            var charge = new StripeChargeCreateOptions
            {
                Amount = (context.Payment.Sum * 100).Round(),
                Description = context.Store.Id,
                Currency = context.Store.DefaultCurrency.ToLower(),
                SourceTokenOrExistingSourceId = stripeTokenId
            };

            var chargeService = new StripeChargeService(ApiSecretKey);
            var chargeResult = chargeService.Create(charge);

            if (!string.IsNullOrEmpty(chargeResult.FailureCode))
            {
                result.ErrorMessage = chargeResult.FailureCode;
                return result;
            }

            result.IsSuccess = true;
            result.OuterId = chargeResult.Id;
            result.OrderId = context.Order.Id;
            result.NewPaymentStatus = PaymentStatus.Paid;
            context.Payment.PaymentStatus = PaymentStatus.Paid;
            context.Payment.OuterId = result.OuterId;

            return result;
        }

        public override VoidProcessPaymentResult VoidProcessPayment(VoidProcessPaymentEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public override CaptureProcessPaymentResult CaptureProcessPayment(CaptureProcessPaymentEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public override RefundProcessPaymentResult RefundProcessPayment(RefundProcessPaymentEvaluationContext context)
        {
            throw new NotImplementedException();
        }

        public override ValidatePostProcessRequestResult ValidatePostProcessRequest(NameValueCollection queryString)
        {
            return new ValidatePostProcessRequestResult
            {
                IsSuccess = true
            };
        }
    }
}