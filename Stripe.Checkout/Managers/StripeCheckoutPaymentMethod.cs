using System;
using System.Collections.Specialized;
using Stripe.Checkout.Helpers;
using VirtoCommerce.Domain.Order.Model;
using VirtoCommerce.Domain.Payment.Model;
using VirtoCommerce.Domain.Store.Model;

namespace Stripe.Checkout.Managers
{
    public class StripeCheckoutPaymentMethod : PaymentMethod
    {
        private const string _stripeTokenAttrName = "token";

        private const string _stripeModeStoreSetting = "Stripe.Checkout.Mode";
        private const string _publishableKeySetting = "Stripe.Checkout.ApiPublishableKey";
        private const string _secretKeySetting = "Stripe.Checkout.ApiSecretKey";
        private const string _paymentMode = "Stripe.Checkout.PaymentActionType";

        private const string _saleMode = "Sale";

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
               return GetSetting(_publishableKeySetting);
            }
        }

        private string ApiSecretKey
        {
            get
            {
                return GetSetting(_secretKeySetting);
            }
        }

        public string PaymentMode
        {
            get
            {
                return GetSetting(_paymentMode);
            }
        }

        public bool IsSaleMode
        {
            get { return PaymentMode == _saleMode; }
        }

        public override PaymentMethodType PaymentMethodType
        {
            get { return PaymentMethodType.Unknown; }
        }

        public override PaymentMethodGroupType PaymentMethodGroupType
        {
            get { return PaymentMethodGroupType.Alternative; }
        }

        public StripeCheckoutPaymentMethod() : base("StripeCheckout")
        {
        }

        public override ProcessPaymentResult ProcessPayment(ProcessPaymentEvaluationContext context)
        {
            var result = new ProcessPaymentResult();

            var stripeTokenId = GetTokenId(context.Parameters);
            if (string.IsNullOrEmpty(stripeTokenId))
            {
                result.Error = "NoStripeTokenPresent";
                return result;
            }

            result = ProcessPayment(context.Payment, context.Store, stripeTokenId);
            return result;
        }

        public override PostProcessPaymentResult PostProcessPayment(PostProcessPaymentEvaluationContext context)
        {
            var result = new PostProcessPaymentResult();

            var stripeTokenId = GetTokenId(context.Parameters);

            var  processPaymentResult = ProcessPayment(context.Payment, context.Store, stripeTokenId);
            result.IsSuccess = processPaymentResult.IsSuccess;
            result.OuterId = processPaymentResult.OuterId;
            result.NewPaymentStatus = processPaymentResult.NewPaymentStatus;
            result.OrderId = context.Order.Id;

            return result;
        }

        private ProcessPaymentResult ProcessPayment(PaymentIn payment, Store store, string token) 
        {
            var result = new ProcessPaymentResult();
            
            var stripeChargeResult = CreateStipeCharge(payment, store, token, IsSaleMode);
            result.OuterId = payment.OuterId = stripeChargeResult.Id;

            if (string.IsNullOrEmpty(stripeChargeResult.FailureCode))
            {
                result.NewPaymentStatus = PaymentStatus.Paid;
                result.IsSuccess = true;

                if (IsSaleMode)
                {
                    result.NewPaymentStatus = payment.PaymentStatus = PaymentStatus.Paid;
                    payment.IsApproved = true;
                    payment.CapturedDate = DateTime.UtcNow;
                }
                else
                {
                    result.NewPaymentStatus = payment.PaymentStatus = PaymentStatus.Authorized;
                    payment.AuthorizedDate = DateTime.UtcNow;
                }
            }
            else
            {
                payment.Comment = $"Stripe charge failed. Charge Id: {stripeChargeResult.Id}, " +
                                          $"Error code: {stripeChargeResult.FailureCode}, " +
                                          $"Error Message: {stripeChargeResult.FailureMessage}";

                result.Error = stripeChargeResult.FailureMessage;
            }

            result.IsSuccess = true;
            return result;
        }

        private StripeCharge CreateStipeCharge(PaymentIn payment, Store store, string stripeTokenId, bool capture = true)
        {
            var charge = new StripeChargeCreateOptions
            {
                Amount = payment.Sum.ToInt(),
                Description = store.Id,
                Currency = store.DefaultCurrency.ToLower(),
                SourceTokenOrExistingSourceId = stripeTokenId,
                Capture = capture
            };

            var chargeResult = new StripeCharge();
            try
            {
                var chargeService = new StripeChargeService(ApiSecretKey);
                chargeResult = chargeService.Create(charge);
            }
            catch (StripeException ex)
            {
                chargeResult.Id = ex.StripeError.ChargeId;
                chargeResult.FailureCode = ex.StripeError.Code;
                chargeResult.FailureMessage = ex.StripeError.Message;
            }
            return chargeResult;
        }

        public override CaptureProcessPaymentResult CaptureProcessPayment(CaptureProcessPaymentEvaluationContext context)
        {
            var result = new CaptureProcessPaymentResult();
            if (string.IsNullOrEmpty(context.Payment.OuterId))
            {
                result.ErrorMessage = "NoStripePaymentIdPresent";
                return result;
            }

            var chargeresult = CaptureCharge(context.Payment.OuterId);

            result.NewPaymentStatus = context.Payment.PaymentStatus = PaymentStatus.Paid;
            context.Payment.CapturedDate = DateTime.UtcNow;
            context.Payment.IsApproved = true;
            result.IsSuccess = true;
            result.OuterId = context.Payment.OuterId = chargeresult.Id;

            return result;
        }

        private StripeCharge CaptureCharge(string stripeChargeId)
        {
            var chargeResult = new StripeCharge();
            try
            {
                var chargeService = new StripeChargeService(ApiSecretKey);
                chargeResult = chargeService.Capture(stripeChargeId);
            }
            catch (StripeException ex)
            {
                chargeResult.Id = ex.StripeError.ChargeId;
                chargeResult.FailureCode = ex.StripeError.Code;
                chargeResult.FailureMessage = ex.StripeError.Message;
            }
            return chargeResult;

        }

        public override VoidProcessPaymentResult VoidProcessPayment(VoidProcessPaymentEvaluationContext context)
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
                IsSuccess = GetTokenId(queryString) != null
            };
        }

        private string GetTokenId(NameValueCollection queryString)
        {
            if (queryString == null || !queryString.HasKeys())
            {
                return null;
            }

            return queryString.Get(_stripeTokenAttrName);
        }
    }
}