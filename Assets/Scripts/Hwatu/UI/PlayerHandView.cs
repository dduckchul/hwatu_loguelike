using System;
using System.Collections.Generic;
using Hwatu.Cards;
using UnityEngine;

namespace Hwatu.UI
{
    [DisallowMultipleComponent]
    public sealed class PlayerHandView : MonoBehaviour
    {
        private const int MaximumSelectedCardCount = 2;

        [SerializeField] private CardView cardPrefab;
        [SerializeField] private RectTransform cardSlot;
        [SerializeField] private FanCardLayout fanCardLayout;
        [SerializeField] private CardCatalogData cardCatalog;

        private readonly List<CardView> cardViews = new List<CardView>();
        private readonly List<CardView> selectedCardViews = new List<CardView>();

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
                cardView.Clicked += HandleCardClicked;
                cardViews.Add(cardView);
            }

            fanCardLayout.RefreshLayout();
        }

        public void Clear()
        {
            selectedCardViews.Clear();

            foreach (CardView cardView in cardViews)
            {
                if (cardView == null)
                {
                    continue;
                }

                cardView.Clicked -= HandleCardClicked;
                cardView.SetSelected(false);
                cardView.gameObject.SetActive(false);
                Destroy(cardView.gameObject);
            }

            cardViews.Clear();

            if (fanCardLayout != null)
            {
                fanCardLayout.RefreshLayout();
            }
        }

        private void HandleCardClicked(CardView cardView)
        {
            if (cardView == null || !cardViews.Contains(cardView))
            {
                return;
            }

            if (cardView.IsSelected)
            {
                selectedCardViews.Remove(cardView);
                cardView.SetSelected(false);
                return;
            }

            if (selectedCardViews.Count >= MaximumSelectedCardCount)
            {
                return;
            }

            selectedCardViews.Add(cardView);
            cardView.SetSelected(true);
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
