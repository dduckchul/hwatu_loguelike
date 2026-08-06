using System;
using Hwatu.Hands;

namespace Hwatu.Combat
{
    // To-do 세륙, 장사 등 특수족보 더 넣기?
    public sealed class HandDamageCalculator
    {
        public const int BaseStake = 5;
        public const int NamedHandBonus = 5;
        public const int DdangBonus = 10;
        public const int BrightDdangBonus = 20;

        public int Calculate(HandResult hand)
        {
            if (hand == null)
            {
                throw new ArgumentNullException(nameof(hand));
            }

            switch (hand.Type)
            {
                case HandType.Ggeut:
                    return BaseStake + hand.Ggeut;
                case HandType.SeRyuk:
                case HandType.JangSa:
                case HandType.JangBbing:
                case HandType.GuBbing:
                case HandType.DokSa:
                case HandType.Ali:
                    return BaseStake + GetMonthSum(hand) + NamedHandBonus;
                case HandType.Ddang:
                    return BaseStake + GetMonthSum(hand) + DdangBonus;
                case HandType.BrightDdang:
                    return BaseStake + GetMonthSum(hand) + BrightDdangBonus;
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
