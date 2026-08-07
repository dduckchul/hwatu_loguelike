using System;
using System.Collections.Generic;
using Hwatu.Cards;
using UnityEngine;

namespace Hwatu.UI
{
    [DisallowMultipleComponent]
    public sealed class CardStoreView : MonoBehaviour
    {
        [SerializeField] private CardView cardPrefab;
        [SerializeField] private CardStoreSlotView[] rewardSlots =
            new CardStoreSlotView[3];
        [SerializeField, Range(0f, 1f)] private float purchasedAlpha = 0.35f;

        private readonly List<CardPresentation> cardPresentations = new List<CardPresentation>();

        private sealed class CardPresentation
        {
            public CardView View { get; }
            public CardData CardData { get; }
            public CanvasGroup CanvasGroup { get; }
            public CardStoreSlotView SlotView { get; }
            public bool IsPurchased { get; set; }

            public CardPresentation(
                CardView view,
                CardData cardData,
                CanvasGroup canvasGroup,
                CardStoreSlotView slotView)
            {
                View = view;
                CardData = cardData;
                CanvasGroup = canvasGroup;
                SlotView = slotView;
            }
        }

        public event Action<CardData> PurchaseRequested;
        public bool IsOpen => gameObject.activeSelf;
        public int CardSlotCount => rewardSlots == null ? 0 : rewardSlots.Length;

        public void Show(IReadOnlyList<CardData> rewards, int cardCost)
        {
            if (rewards == null)
            {
                throw new ArgumentNullException(nameof(rewards));
            }

            ValidateReferences();

            if (rewards.Count != CardSlotCount)
            {
                throw new ArgumentException(
                    $"Card store view requires exactly {CardSlotCount} cards.",
                    nameof(rewards));
            }

            ClearCards();
            gameObject.SetActive(true);

            for (int index = 0; index < rewards.Count; index++)
            {
                CardData cardData = rewards[index];
                if (cardData == null)
                {
                    throw new ArgumentException("Card rewards cannot contain a null card.", nameof(rewards));
                }

                CardStoreSlotView rewardSlot = rewardSlots[index];
                rewardSlot.ResetSlot(cardCost);

                var card = new CardInstance(cardData.ToDefinition());
                CardView cardView = Instantiate(cardPrefab, rewardSlot.CardRoot);
                cardView.Bind(card, cardData);
                cardView.Clicked += HandleCardClicked;

                CanvasGroup canvasGroup = cardView.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = cardView.gameObject.AddComponent<CanvasGroup>();
                }

                cardPresentations.Add(
                    new CardPresentation(
                        cardView,
                        cardData,
                        canvasGroup,
                        rewardSlot));
            }
        }

        public void RefreshCardCost(int cardCost)
        {
            foreach (CardPresentation presentation in cardPresentations)
            {
                if (!presentation.IsPurchased)
                {
                    presentation.SlotView.SetCost(cardCost);
                }
            }
        }

        public void Hide()
        {
            ClearCards();
            gameObject.SetActive(false);
        }

        public void MarkPurchased(CardData cardData)
        {
            if (cardData == null)
            {
                throw new ArgumentNullException(nameof(cardData));
            }

            CardPresentation presentation = FindPresentation(cardData);
            if (presentation == null)
            {
                throw new InvalidOperationException(
                    $"Card '{cardData.CardId}' is not displayed in the store.");
            }

            if (presentation.IsPurchased)
            {
                throw new InvalidOperationException(
                    $"Card '{cardData.CardId}' is already purchased.");
            }

            presentation.IsPurchased = true;
            presentation.View.SetSelected(false);
            presentation.View.SetInteractionEnabled(false);
            presentation.CanvasGroup.alpha = purchasedAlpha;
            presentation.SlotView.MarkSoldOut();
        }

        private void HandleCardClicked(CardView clickedCardView)
        {
            CardPresentation clickedCard = FindPresentation(clickedCardView);
            if (clickedCard == null)
            {
                return;
            }

            if (!clickedCard.IsPurchased)
            {
                PurchaseRequested?.Invoke(clickedCard.CardData);
            }
        }

        private void ClearCards()
        {
            foreach (CardPresentation presentation in cardPresentations)
            {
                CardView cardView = presentation.View;
                if (cardView == null)
                {
                    continue;
                }

                cardView.Clicked -= HandleCardClicked;
                cardView.gameObject.SetActive(false);
                Destroy(cardView.gameObject);
            }

            cardPresentations.Clear();
        }

        private CardPresentation FindPresentation(CardView cardView)
        {
            if (cardView == null)
            {
                return null;
            }

            foreach (CardPresentation presentation in cardPresentations)
            {
                if (presentation.View == cardView)
                {
                    return presentation;
                }
            }

            return null;
        }

        private CardPresentation FindPresentation(CardData cardData)
        {
            foreach (CardPresentation presentation in cardPresentations)
            {
                if (presentation.CardData == cardData)
                {
                    return presentation;
                }
            }

            return null;
        }

        private void ValidateReferences()
        {
            if (cardPrefab == null)
            {
                throw new InvalidOperationException("Reward card prefab is not assigned.");
            }

            if (rewardSlots == null || rewardSlots.Length == 0)
            {
                throw new InvalidOperationException("At least one reward slot must be assigned.");
            }

            for (int index = 0; index < rewardSlots.Length; index++)
            {
                if (rewardSlots[index] == null)
                {
                    throw new InvalidOperationException($"Reward slot {index + 1} is not assigned.");
                }
            }
        }
    }
}
