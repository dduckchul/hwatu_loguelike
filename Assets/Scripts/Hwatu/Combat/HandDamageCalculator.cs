using System;
using Hwatu.Hands;

namespace Hwatu.Combat
{
    public sealed class HandDamageCalculator
    {
        public const int NamedHandBonus = 5;
        public const int DdangBonus = 10;
        public const int BrightDdangBonus = 20;
        public const int TypeSetBonus = 5;

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

            int baseDamage;
            switch (hand.Type)
            {
                case HandType.Ggeut:
                    baseDamage = baseStake + hand.Ggeut;
                    break;
                case HandType.SeRyuk:
                case HandType.JangSa:
                case HandType.JangBbing:
                case HandType.GuBbing:
                case HandType.DokSa:
                case HandType.Ali:
                    baseDamage = baseStake + GetMonthSum(hand) + NamedHandBonus;
                    break;
                case HandType.Ddang:
                    baseDamage = baseStake + GetMonthSum(hand) + DdangBonus;
                    break;
                case HandType.BrightDdang:
                    baseDamage = baseStake + GetMonthSum(hand) + BrightDdangBonus;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(hand), hand.Type, null);
            }

            return checked(baseDamage + GetTypeSetBonus(hand.Tags));
        }

        private static int GetMonthSum(HandResult hand)
        {
            return checked(
                hand.FirstCard.Definition.Month
                + hand.SecondCard.Definition.Month);
        }

        private static int GetTypeSetBonus(HandTag tags)
        {
            bool hasRibbonSet = (tags & HandTag.RibbonSet) != 0;
            bool hasAnimalSet = (tags & HandTag.AnimalSet) != 0;
            if (hasRibbonSet && hasAnimalSet)
            {
                throw new InvalidOperationException(
                    "A hand cannot have both RibbonSet and AnimalSet tags.");
            }

            return hasRibbonSet || hasAnimalSet ? TypeSetBonus : 0;
        }
    }
}
