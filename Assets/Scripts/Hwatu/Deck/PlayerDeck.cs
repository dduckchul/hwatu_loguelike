using System;
using System.Collections.Generic;
using Hwatu.Cards;
using Hwatu.Randomness;

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

        public CardInstance UpgradeCard(
            CardInstance currentCard,
            CardDefinition upgradedDefinition)
        {
            if (currentCard == null)
            {
                throw new ArgumentNullException(nameof(currentCard));
            }

            if (upgradedDefinition == null)
            {
                throw new ArgumentNullException(nameof(upgradedDefinition));
            }

            int cardIndex = cards.IndexOf(currentCard);
            if (cardIndex < 0)
            {
                throw new InvalidOperationException("The card to upgrade is not in the player deck.");
            }

            if (currentCard.Definition.CardType != CardType.Normal)
            {
                throw new InvalidOperationException("Only Normal cards can be upgraded.");
            }

            if (currentCard.Definition.Month != upgradedDefinition.Month)
            {
                throw new InvalidOperationException("An upgraded card must keep the original month.");
            }

            if (!IsUpgradeCardType(upgradedDefinition.CardType))
            {
                throw new InvalidOperationException(
                    "An upgraded card must be Bright, Ribbon, or Animal.");
            }

            var upgradedCard = new CardInstance(upgradedDefinition);
            cards[cardIndex] = upgradedCard;
            return upgradedCard;
        }

        public BattleDeck CreateBattleDeck(IRandomSource randomSource)
        {
            return new BattleDeck(cards, randomSource);
        }

        private static bool IsUpgradeCardType(CardType cardType)
        {
            return cardType == CardType.Bright
                || cardType == CardType.Ribbon
                || cardType == CardType.Animal;
        }
    }
}
