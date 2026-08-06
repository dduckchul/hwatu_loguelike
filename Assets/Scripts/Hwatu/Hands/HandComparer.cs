using System;

namespace Hwatu.Hands
{
    public enum HandComparisonResult
    {
        FirstWins,
        Draw,
        SecondWins
    }

    public static class HandComparer
    {
        public static HandComparisonResult Compare(HandResult first, HandResult second)
        {
            if (first == null)
            {
                throw new ArgumentNullException(nameof(first));
            }

            if (second == null)
            {
                throw new ArgumentNullException(nameof(second));
            }

            if (first.Rank > second.Rank)
            {
                return HandComparisonResult.FirstWins;
            }

            if (first.Rank < second.Rank)
            {
                return HandComparisonResult.SecondWins;
            }

            return HandComparisonResult.Draw;
        }
    }
}
