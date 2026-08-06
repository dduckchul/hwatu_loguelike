using System;
using Hwatu.Cards;

namespace Hwatu.Hands
{
    public sealed class HandResult
    {
        public HandType Type { get; }
        public int Rank { get; }
        public int Ggeut { get; }
        public HandTag Tags { get; }
        public CardInstance FirstCard { get; }
        public CardInstance SecondCard { get; }

        public HandResult(
            HandType type,
            int rank,
            int ggeut,
            HandTag tags,
            CardInstance firstCard,
            CardInstance secondCard)
        {
            if (rank < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rank));
            }

            if (ggeut < 0 || ggeut > 9)
            {
                throw new ArgumentOutOfRangeException(nameof(ggeut));
            }

            Type = type;
            Rank = rank;
            Ggeut = ggeut;
            Tags = tags;
            FirstCard = firstCard ?? throw new ArgumentNullException(nameof(firstCard));
            SecondCard = secondCard ?? throw new ArgumentNullException(nameof(secondCard));
        }
    }
}
