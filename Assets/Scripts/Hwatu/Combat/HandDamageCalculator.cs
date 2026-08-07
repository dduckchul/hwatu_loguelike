using System;
using Hwatu.Hands;

namespace Hwatu.Combat
{
    // To-do 세륙, 장사 등 특수족보 더 넣기?
    public sealed class HandDamageCalculator
    {
        public const int NamedHandBonus = 5;
        public const int DdangBonus = 10;
        public const int BrightDdangBonus = 20;

        public int Calculate(HandResult hand, int baseStake)
        {
            if (hand == null)
            {
                throw new ArgumentNullException(nameof(hand));
            }

            if (baseStake < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(baseStake),
                    baseStake,
                    "Base stake cannot be negative.");
            }

            switch (hand.Type)
            {
                case HandType.Ggeut:
                    return baseStake + hand.Ggeut;
                case HandType.SeRyuk:
                case HandType.JangSa:
                case HandType.JangBbing:
                case HandType.GuBbing:
                case HandType.DokSa:
                case HandType.Ali:
                    return baseStake + GetMonthSum(hand) + NamedHandBonus;
                case HandType.Ddang:
                    return baseStake + GetMonthSum(hand) + DdangBonus;
                case HandType.BrightDdang:
                    return baseStake + GetMonthSum(hand) + BrightDdangBonus;
                default:
                    throw new ArgumentOutOfRangeException(nameof(hand), hand.Type, null);
            }
        }

        private static int GetMonthSum(HandResult hand)
        {
            return checked(
                hand.FirstCard.Definition.Month
                + hand.SecondCard.Definition.Month);
        }
    }
}
