using System;
using System.Collections.Generic;
using Hwatu.Cards;

namespace Hwatu.Deck
{
    public sealed class PlayerDeck
    {
        private readonly List<CardInstance> cards;
        private readonly IReadOnlyList<CardInstance> readOnlyCards;

        public IReadOnlyList<CardInstance> Cards => readOnlyCards;
        public int CardCount => cards.Count;

        public PlayerDeck(IEnumerable<CardInstance> initialCards)
        {
            if (initialCards == null)
            {
                throw new ArgumentNullException(nameof(initialCards));
            }

            cards = new List<CardInstance>();
            foreach (CardInstance card in initialCards)
            {
                AddCard(card);
            }

            readOnlyCards = cards.AsReadOnly();
        }

        public void AddCard(CardInstance card)
        {
            if (card == null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            cards.Add(card);
        }

        public BattleDeck CreateBattleDeck(IRandomSource randomSource)
        {
            return new BattleDeck(cards, randomSource);
        }
    }
}
