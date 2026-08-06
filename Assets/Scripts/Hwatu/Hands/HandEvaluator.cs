using System;
using Hwatu.Cards;

namespace Hwatu.Hands
{
    public sealed class HandEvaluator
    {
        private const int NamedRankBase = 10;
        private const int DdangRankBase = 20;
        private const int BrightDdangRankBase = 40;

        public HandResult Evaluate(CardInstance firstCard, CardInstance secondCard)
        {
            if (firstCard == null)
            {
                throw new ArgumentNullException(nameof(firstCard));
            }

            if (secondCard == null)
            {
                throw new ArgumentNullException(nameof(secondCard));
            }

            int firstMonth = firstCard.Definition.Month;
            int secondMonth = secondCard.Definition.Month;
            int lowMonth = Math.Min(firstMonth, secondMonth);
            int highMonth = Math.Max(firstMonth, secondMonth);
            int ggeut = (firstMonth + secondMonth) % 10;

            int brightDdangOrder = GetBrightDdangOrder(firstCard, secondCard, lowMonth, highMonth);
            if (brightDdangOrder > 0)
            {
                return CreateResult(
                    HandType.BrightDdang,
                    BrightDdangRankBase + brightDdangOrder,
                    ggeut,
                    HandTag.Pair | HandTag.Bright,
                    firstCard,
                    secondCard);
            }

            if (firstMonth == secondMonth)
            {
                return CreateResult(
                    HandType.Ddang,
                    DdangRankBase + firstMonth,
                    ggeut,
                    HandTag.Pair,
                    firstCard,
                    secondCard);
            }

            HandType namedType;
            int namedOrder;
            if (TryGetNamedHand(lowMonth, highMonth, out namedType, out namedOrder))
            {
                return CreateResult(
                    namedType,
                    NamedRankBase + namedOrder,
                    ggeut,
                    HandTag.Named,
                    firstCard,
                    secondCard);
            }

            return CreateResult(
                HandType.Ggeut,
                ggeut,
                ggeut,
                HandTag.None,
                firstCard,
                secondCard);
        }

        private static int GetBrightDdangOrder(
            CardInstance firstCard,
            CardInstance secondCard,
            int lowMonth,
            int highMonth)
        {
            bool bothBright = firstCard.Definition.CardType == CardType.Bright
                && secondCard.Definition.CardType == CardType.Bright;

            if (!bothBright)
            {
                return 0;
            }

            if (lowMonth == 1 && highMonth == 3)
            {
                return 1;
            }

            if (lowMonth == 1 && highMonth == 8)
            {
                return 2;
            }

            return lowMonth == 3 && highMonth == 8 ? 3 : 0;
        }

        private static bool TryGetNamedHand(
            int lowMonth,
            int highMonth,
            out HandType type,
            out int order)
        {
            if (lowMonth == 4 && highMonth == 6)
            {
                type = HandType.SeRyuk;
                order = 1;
                return true;
            }

            if (lowMonth == 4 && highMonth == 10)
            {
                type = HandType.JangSa;
                order = 2;
                return true;
            }

            if (lowMonth == 1 && highMonth == 10)
            {
                type = HandType.JangBbing;
                order = 3;
                return true;
            }

            if (lowMonth == 1 && highMonth == 9)
            {
                type = HandType.GuBbing;
                order = 4;
                return true;
            }

            if (lowMonth == 1 && highMonth == 4)
            {
                type = HandType.DokSa;
                order = 5;
                return true;
            }

            if (lowMonth == 1 && highMonth == 2)
            {
                type = HandType.Ali;
                order = 6;
                return true;
            }

            type = HandType.Ggeut;
            order = 0;
            return false;
        }

        private static HandResult CreateResult(
            HandType type,
            int rank,
            int ggeut,
            HandTag tags,
            CardInstance firstCard,
            CardInstance secondCard)
        {
            return new HandResult(type, rank, ggeut, tags, firstCard, secondCard);
        }
    }
}
