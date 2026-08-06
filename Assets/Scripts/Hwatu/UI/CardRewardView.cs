using System;
using System.Collections;
using System.Collections.Generic;
using Hwatu.Cards;
using UnityEngine;
using UnityEngine.UI;

namespace Hwatu.UI
{
    [DisallowMultipleComponent]
    public sealed class CardRewardView : MonoBehaviour
    {
        private const int RewardSlotCount = 3;

        [SerializeField] private CardView cardPrefab;
        [SerializeField] private RectTransform[] rewardSlots = new RectTransform[RewardSlotCount];
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button skipButton;

        [Header("Selection Motion")]
        [SerializeField, Min(0f)] private float selectedLiftDistance = 24f;
        [SerializeField, Range(0f, 1f)] private float unselectedAlpha = 0.65f;
        [SerializeField, Min(0f)] private float selectionTransitionDuration = 0.12f;

        private readonly List<CardView> cardViews = new List<CardView>();
        private readonly List<CardPresentation> cardPresentations = new List<CardPresentation>();
        private readonly Dictionary<CardView, CardData> cardDataByView =
            new Dictionary<CardView, CardData>();
        private CardData selectedCard;
        private Coroutine selectionTransition;

        private sealed class CardPresentation
        {
            public CardView View { get; }
            public RectTransform RectTransform { get; }
            public CanvasGroup CanvasGroup { get; }
            public Vector2 RestingPosition { get; }

            public CardPresentation(
                CardView view,
                RectTransform rectTransform,
                CanvasGroup canvasGroup)
            {
                View = view;
                RectTransform = rectTransform;
                CanvasGroup = canvasGroup;
                RestingPosition = rectTransform.anchoredPosition;
            }
        }

        public event Action<CardData> RewardConfirmed;
        public event Action RewardSkipped;
        public bool IsOpen => gameObject.activeSelf;

        public void Show(IReadOnlyList<CardData> rewards)
        {
            if (rewards == null)
            {
                throw new ArgumentNullException(nameof(rewards));
            }

            if (rewards.Count != RewardSlotCount)
            {
                throw new ArgumentException(
                    $"Card reward view requires exactly {RewardSlotCount} cards.",
                    nameof(rewards));
            }

            ValidateReferences();
            ClearCards();
            selectedCard = null;
            gameObject.SetActive(true);

            confirmButton.onClick.RemoveListener(HandleConfirmClicked);
            skipButton.onClick.RemoveListener(HandleSkipClicked);
            confirmButton.onClick.AddListener(HandleConfirmClicked);
            skipButton.onClick.AddListener(HandleSkipClicked);
            SetConfirmInteraction(false);

            for (int index = 0; index < rewards.Count; index++)
            {
                CardData cardData = rewards[index];
                if (cardData == null)
                {
                    throw new ArgumentException("Card rewards cannot contain a null card.", nameof(rewards));
                }

                var card = new CardInstance(cardData.ToDefinition());
                CardView cardView = Instantiate(cardPrefab, rewardSlots[index]);
                cardView.Bind(card, cardData);
                cardView.Clicked += HandleCardClicked;
                cardViews.Add(cardView);
                cardDataByView.Add(cardView, cardData);

                RectTransform cardRectTransform = (RectTransform)cardView.transform;
                CanvasGroup canvasGroup = cardView.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = cardView.gameObject.AddComponent<CanvasGroup>();
                }

                cardPresentations.Add(
                    new CardPresentation(cardView, cardRectTransform, canvasGroup));
            }
        }

        public void Hide()
        {
            ClearCards();
            selectedCard = null;

            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveListener(HandleConfirmClicked);
            }

            if (skipButton != null)
            {
                skipButton.onClick.RemoveListener(HandleSkipClicked);
            }

            gameObject.SetActive(false);
        }

        private void HandleCardClicked(CardView clickedCardView)
        {
            if (clickedCardView == null || !cardViews.Contains(clickedCardView))
            {
                return;
            }

            foreach (CardView cardView in cardViews)
            {
                cardView.SetSelected(cardView == clickedCardView);
            }

            selectedCard = cardDataByView[clickedCardView];
            SetConfirmInteraction(selectedCard != null);
            PlaySelectionTransition(clickedCardView);
        }

        private void HandleConfirmClicked()
        {
            if (selectedCard != null)
            {
                RewardConfirmed?.Invoke(selectedCard);
            }
        }

        private void HandleSkipClicked()
        {
            RewardSkipped?.Invoke();
        }

        private void ClearCards()
        {
            if (selectionTransition != null)
            {
                StopCoroutine(selectionTransition);
                selectionTransition = null;
            }

            foreach (CardView cardView in cardViews)
            {
                if (cardView == null)
                {
                    continue;
                }

                cardView.Clicked -= HandleCardClicked;
                cardView.gameObject.SetActive(false);
                Destroy(cardView.gameObject);
            }

            cardViews.Clear();
            cardPresentations.Clear();
            cardDataByView.Clear();
        }

        private void PlaySelectionTransition(CardView selectedView)
        {
            if (selectionTransition != null)
            {
                StopCoroutine(selectionTransition);
            }

            selectionTransition = StartCoroutine(AnimateSelection(selectedView));
        }

        private IEnumerator AnimateSelection(CardView selectedView)
        {
            var startPositions = new Vector2[cardPresentations.Count];
            var startAlphas = new float[cardPresentations.Count];
            for (int index = 0; index < cardPresentations.Count; index++)
            {
                CardPresentation presentation = cardPresentations[index];
                startPositions[index] = presentation.RectTransform.anchoredPosition;
                startAlphas[index] = presentation.CanvasGroup.alpha;
            }

            float elapsed = 0f;
            do
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = selectionTransitionDuration <= 0f
                    ? 1f
                    : Mathf.Clamp01(elapsed / selectionTransitionDuration);
                float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

                for (int index = 0; index < cardPresentations.Count; index++)
                {
                    CardPresentation presentation = cardPresentations[index];
                    bool isSelected = presentation.View == selectedView;
                    Vector2 targetPosition = presentation.RestingPosition
                        + (isSelected ? Vector2.up * selectedLiftDistance : Vector2.zero);
                    float targetAlpha = isSelected ? 1f : unselectedAlpha;

                    presentation.RectTransform.anchoredPosition = Vector2.Lerp(
                        startPositions[index],
                        targetPosition,
                        easedProgress);
                    presentation.CanvasGroup.alpha = Mathf.Lerp(
                        startAlphas[index],
                        targetAlpha,
                        easedProgress);
                }

                if (progress >= 1f)
                {
                    break;
                }

                yield return null;
            }
            while (true);

            selectionTransition = null;
        }

        private void SetConfirmInteraction(bool isEnabled)
        {
            confirmButton.interactable = isEnabled;
            RewardButtonView buttonView = confirmButton.GetComponent<RewardButtonView>();
            if (buttonView != null)
            {
                buttonView.SetInteractionEnabled(isEnabled);
            }
        }

        private void ValidateReferences()
        {
            if (cardPrefab == null)
            {
                throw new InvalidOperationException("Reward card prefab is not assigned.");
            }

            if (rewardSlots == null || rewardSlots.Length != RewardSlotCount)
            {
                throw new InvalidOperationException(
                    $"Exactly {RewardSlotCount} reward slots must be assigned.");
            }

            for (int index = 0; index < rewardSlots.Length; index++)
            {
                if (rewardSlots[index] == null)
                {
                    throw new InvalidOperationException($"Reward slot {index + 1} is not assigned.");
                }
            }

            if (confirmButton == null || skipButton == null)
            {
                throw new InvalidOperationException("Reward confirm and skip buttons must be assigned.");
            }
        }
    }
}
