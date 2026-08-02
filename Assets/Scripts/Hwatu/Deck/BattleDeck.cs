using System;
using System.Collections.Generic;
using Hwatu.Cards;

namespace Hwatu.Deck
{
    public sealed class BattleDeck
    {
        public const int DefaultHandSize = 3;

        private readonly List<CardInstance> drawPile;
        private readonly List<CardInstance> hand = new List<CardInstance>();
        private readonly List<CardInstance> discardPile = new List<CardInstance>();
        private readonly IReadOnlyList<CardInstance> readOnlyHand;
        private readonly IRandomSource randomSource;

        public IReadOnlyList<CardInstance> Hand => readOnlyHand;
        public int DrawPileCount => drawPile.Count;
        public int HandCount => hand.Count;
        public int DiscardPileCount => discardPile.Count;

        public BattleDeck(IEnumerable<CardInstance> cards, IRandomSource randomSource)
        {
            if (cards == null)
            {
                throw new ArgumentNullException(nameof(cards));
            }

            this.randomSource = randomSource
                ?? throw new ArgumentNullException(nameof(randomSource));

            drawPile = new List<CardInstance>();
            foreach (CardInstance card in cards)
            {
                if (card == null)
                {
                    throw new ArgumentException("Battle deck cannot contain a null card.", nameof(cards));
                }

                drawPile.Add(card);
            }

            readOnlyHand = hand.AsReadOnly();
            Shuffle(drawPile);
        }

        public int DrawToHand(int targetHandSize = DefaultHandSize)
        {
            if (targetHandSize < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(targetHandSize));
            }

            int drawnCount = 0;
            while (hand.Count < targetHandSize)
            {
                if (drawPile.Count == 0 && !RecycleDiscardPile())
                {
                    break;
                }

                int topIndex = drawPile.Count - 1;
                hand.Add(drawPile[topIndex]);
                drawPile.RemoveAt(topIndex);
                drawnCount++;
            }

            return drawnCount;
        }

        public void DiscardHand()
        {
            discardPile.AddRange(hand);
            hand.Clear();
        }

        private bool RecycleDiscardPile()
        {
            if (discardPile.Count == 0)
            {
                return false;
            }

            drawPile.AddRange(discardPile);
            discardPile.Clear();
            Shuffle(drawPile);
            return true;
        }

        private void Shuffle(List<CardInstance> cards)
        {
            for (int index = cards.Count - 1; index > 0; index--)
            {
                int swapIndex = randomSource.Next(0, index + 1);
                CardInstance temporary = cards[index];
                cards[index] = cards[swapIndex];
                cards[swapIndex] = temporary;
            }
        }
    }
}
