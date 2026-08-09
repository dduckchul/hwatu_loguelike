using System;
using System.Collections.Generic;
using Hwatu.Cards;
using Hwatu.Randomness;
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
        [SerializeField] private RunRandomProvider runRandomProvider;

        public PlayerDeck Deck { get; private set; }

        private void Awake()
        {
            Deck = CreateStartingDeck();

            RebuildBattleDeck();
        }

        public void RebuildBattleDeck()
        {
            if (Deck == null)
            {
                throw new InvalidOperationException(
                    "Player deck is not initialized.");
            }

            if (battleDeckController == null)
            {
                throw new InvalidOperationException("Battle deck controller is not assigned.");
            }

            if (runRandomProvider == null)
            {
                throw new InvalidOperationException("Run random provider is not assigned.");
            }

            battleDeckController.Initialize(
                Deck,
                runRandomProvider.GetStream(RandomStreamId.BattleDeck));
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

                if (cardData.Month < CardDefinition.MinMonth
                    || cardData.Month > CardDefinition.MaxMonth)
                {
                    throw new InvalidOperationException(
                        $"Starting card '{cardData.CardId}' has an invalid month.");
                }

                includedMonths[cardData.Month] = true;
            }
        }
    }
}
