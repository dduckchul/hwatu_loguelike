using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hwatu.Cards
{
    [CreateAssetMenu(fileName = "CardCatalogData", menuName = "Hwatu/Card Catalog")]
    public sealed class CardCatalogData : ScriptableObject
    {
        [SerializeField] private List<CardData> cards = new List<CardData>();

        private Dictionary<string, CardData> cardsById;

        public IReadOnlyList<CardData> Cards => cards;

        public CardData GetById(string cardId)
        {
            if (string.IsNullOrWhiteSpace(cardId))
            {
                throw new ArgumentException("Card ID cannot be empty.", nameof(cardId));
            }

            EnsureLookup();

            CardData cardData;
            if (!cardsById.TryGetValue(cardId, out cardData))
            {
                throw new KeyNotFoundException($"Card data with ID '{cardId}' was not found.");
            }

            return cardData;
        }

        private void OnEnable()
        {
            cardsById = null;
        }

        private void OnValidate()
        {
            cardsById = null;
        }

        private void EnsureLookup()
        {
            if (cardsById != null)
            {
                return;
            }

            var lookup = new Dictionary<string, CardData>(StringComparer.Ordinal);

            foreach (CardData cardData in cards)
            {
                if (cardData == null)
                {
                    throw new InvalidOperationException("Card catalog cannot contain a null card.");
                }

                if (string.IsNullOrWhiteSpace(cardData.CardId))
                {
                    throw new InvalidOperationException("Card catalog contains a card with an empty ID.");
                }

                if (lookup.ContainsKey(cardData.CardId))
                {
                    throw new InvalidOperationException(
                        $"Card catalog contains duplicate ID '{cardData.CardId}'.");
                }

                lookup.Add(cardData.CardId, cardData);
            }

            cardsById = lookup;
        }
    }
}
