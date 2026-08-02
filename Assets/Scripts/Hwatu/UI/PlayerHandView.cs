using System;
using System.Collections.Generic;
using Hwatu.Cards;
using UnityEngine;

namespace Hwatu.UI
{
    [DisallowMultipleComponent]
    public sealed class PlayerHandView : MonoBehaviour
    {
        [SerializeField] private CardView cardPrefab;
        [SerializeField] private RectTransform cardSlot;
        [SerializeField] private FanCardLayout fanCardLayout;
        [SerializeField] private CardCatalogData cardCatalog;

        private readonly List<CardView> cardViews = new List<CardView>();

        public int CardCount => cardViews.Count;

        public void SetCards(IReadOnlyList<CardInstance> cards)
        {
            if (cards == null)
            {
                throw new ArgumentNullException(nameof(cards));
            }

            ValidateReferences();
            var resolvedCardData = new List<CardData>(cards.Count);

            foreach (CardInstance card in cards)
            {
                if (card == null)
                {
                    throw new ArgumentException("Player hand cannot contain a null card.", nameof(cards));
                }

                resolvedCardData.Add(cardCatalog.GetById(card.Definition.Id));
            }

            Clear();

            for (int index = 0; index < cards.Count; index++)
            {
                CardView cardView = Instantiate(cardPrefab, cardSlot);
                cardView.Bind(cards[index], resolvedCardData[index]);
                cardViews.Add(cardView);
            }

            fanCardLayout.RefreshLayout();
        }

        public void Clear()
        {
            foreach (CardView cardView in cardViews)
            {
                if (cardView == null)
                {
                    continue;
                }

                cardView.gameObject.SetActive(false);
                Destroy(cardView.gameObject);
            }

            cardViews.Clear();

            if (fanCardLayout != null)
            {
                fanCardLayout.RefreshLayout();
            }
        }

        private void ValidateReferences()
        {
            if (cardPrefab == null)
            {
                throw new InvalidOperationException("Card prefab is not assigned.");
            }

            if (cardSlot == null)
            {
                throw new InvalidOperationException("Card slot is not assigned.");
            }

            if (fanCardLayout == null)
            {
                throw new InvalidOperationException("Fan card layout is not assigned.");
            }

            if (cardCatalog == null)
            {
                throw new InvalidOperationException("Card catalog is not assigned.");
            }
        }
    }
}
