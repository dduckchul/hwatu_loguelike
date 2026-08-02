using System;
using Hwatu.Hands;

namespace Hwatu.UI
{
    public static class HandDisplayName
    {
        public static string Get(HandResult hand)
        {
            if (hand == null)
            {
                throw new ArgumentNullException(nameof(hand));
            }

            switch (hand.Type)
            {
                case HandType.Ggeut:
                    return hand.Ggeut + "끗";
                case HandType.SeRyuk:
                    return "세륙";
                case HandType.JangSa:
                    return "장사";
                case HandType.JangBbing:
                    return "장삥";
                case HandType.GuBbing:
                    return "구삥";
                case HandType.DokSa:
                    return "독사";
                case HandType.Ali:
                    return "알리";
                case HandType.Ddang:
                    return hand.FirstCard.Definition.Month + "땡";
                case HandType.BrightDdang:
                    int lowMonth = Math.Min(
                        hand.FirstCard.Definition.Month,
                        hand.SecondCard.Definition.Month);
                    int highMonth = Math.Max(
                        hand.FirstCard.Definition.Month,
                        hand.SecondCard.Definition.Month);
                    return $"{lowMonth}{highMonth}광땡";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
