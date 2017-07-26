using System;

namespace Stripe.Checkout.Helpers
{
    public static class RoundingHelper
    {
        public static int Round(this decimal value)
        {
            return (int)Math.Round(value, MidpointRounding.AwayFromZero);
        }
    }
}