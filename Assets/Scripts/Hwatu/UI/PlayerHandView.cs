using System;
using System.Collections.Generic;
using Hwatu.Cards;
using Hwatu.Hands;
using TMPro;
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
        [SerializeField] private TMP_Text selectedHandNameText;
        [SerializeField] private TMP_Text selectedCardCountText;

        private readonly List<CardView> cardViews = new List<CardView>();
        private readonly List<CardInstance> selectedCards = new List<CardInstance>();

        public event Action<IReadOnlyList<CardInstance>> SelectionChanged;
        public IReadOnlyList<CardInstance> SelectedCards => selectedCards;

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
            selectedCards.Clear();

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

            SelectionChanged?.Invoke(selectedCards);
        }

        public void RefreshSelectionDisplay(HandResult handResult)
        {
            switch (selectedCards.Count)
            {
                case 0:
                    selectedCardCountText.text = "○○ (0/2)";
                    break;
                case 1:
                    selectedCardCountText.text = "●○ (1/2)";
                    break;
                default:
                    selectedCardCountText.text = "●● (2/2)";
                    break;
            }

            selectedHandNameText.text = handResult == null
                ? "-"
                : HandDisplayName.Get(handResult);
        }

        public void SetInteractionEnabled(bool isEnabled)
        {
            foreach (CardView cardView in cardViews)
            {
                if (cardView != null)
                {
                    cardView.SetInteractionEnabled(isEnabled);
                }
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
                selectedCards.Remove(cardView.Card);
                cardView.SetSelected(false);
                NotifySelectionChanged();
                return;
            }

            if (selectedCards.Count >= MaximumSelectedCardCount)
            {
                return;
            }

            selectedCards.Add(cardView.Card);
            cardView.SetSelected(true);
            NotifySelectionChanged();
        }

        private void NotifySelectionChanged()
        {
            SelectionChanged?.Invoke(selectedCards);
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

            if (selectedHandNameText == null)
            {
                throw new InvalidOperationException("Selected hand name text is not assigned.");
            }

            if (selectedCardCountText == null)
            {
                throw new InvalidOperationException("Selected card count text is not assigned.");
            }
        }
    }
}
