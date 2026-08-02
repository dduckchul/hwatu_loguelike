using System;
using System.Collections.Generic;
using Hwatu.Cards;
using UnityEngine;

namespace Hwatu.Deck
{
    [DisallowMultipleComponent]
    public sealed class PlayerDeckInitializer : MonoBehaviour
    {
        private const int StartingCardCount =
            CardDefinition.MaxMonth - CardDefinition.MinMonth + 1;

        [SerializeField]
        private CardData[] startingCards = new CardData[StartingCardCount];

        [SerializeField] private BattleDeckController battleDeckController;
        [SerializeField] private int battleDeckSeed;

        public PlayerDeck Deck { get; private set; }

        private void Awake()
        {
            Deck = CreateStartingDeck();

            if (battleDeckController == null)
            {
                throw new InvalidOperationException("Battle deck controller is not assigned.");
            }

            battleDeckController.Initialize(Deck, battleDeckSeed);
        }

        private PlayerDeck CreateStartingDeck()
        {
            ValidateStartingCards();

            var cardInstances = new List<CardInstance>(startingCards.Length);
            foreach (CardData cardData in startingCards)
            {
                cardInstances.Add(new CardInstance(cardData.ToDefinition()));
            }

            return new PlayerDeck(cardInstances);
        }

        private void ValidateStartingCards()
        {
            if (startingCards == null || startingCards.Length != StartingCardCount)
            {
                throw new InvalidOperationException(
                    $"Starting deck must contain exactly {StartingCardCount} cards.");
            }

            var includedMonths = new bool[CardDefinition.MaxMonth + 1];

            foreach (CardData cardData in startingCards)
            {
                if (cardData == null)
                {
                    throw new InvalidOperationException("Starting deck cannot contain a null card.");
                }

                if (cardData.CardType != CardType.Normal)
                {
                    throw new InvalidOperationException(
                        $"Starting card '{cardData.CardId}' must be a normal card.");
                }

                if (cardData.Month < CardDefinition.MinMonth
                    || cardData.Month > CardDefinition.MaxMonth)
                {
                    throw new InvalidOperationException(
                        $"Starting card '{cardData.CardId}' has an invalid month.");
                }

                if (includedMonths[cardData.Month])
                {
                    throw new InvalidOperationException(
                        $"Starting deck contains more than one card for month {cardData.Month}.");
                }

                includedMonths[cardData.Month] = true;
            }

            for (int month = CardDefinition.MinMonth; month <= CardDefinition.MaxMonth; month++)
            {
                if (!includedMonths[month])
                {
                    throw new InvalidOperationException(
                        $"Starting deck is missing a normal card for month {month}.");
                }
            }
        }
    }
}
