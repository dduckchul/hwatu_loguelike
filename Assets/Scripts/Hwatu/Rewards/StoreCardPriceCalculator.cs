using System;

namespace Hwatu.Rewards
{
    public sealed class StoreCardPriceCalculator
    {
        public const int PriceIncreasePerPurchase = 20;

        public int Calculate(int purchasedCardCount)
        {
            if (purchasedCardCount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(purchasedCardCount),
                    purchasedCardCount,
                    "Purchased card count cannot be negative.");
            }

            return checked(purchasedCardCount * PriceIncreasePerPurchase);
        }
    }
}
