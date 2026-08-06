using System;
using Hwatu.Cards;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hwatu.UI
{
    [DisallowMultipleComponent]
    public sealed class CardView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private float ScaleConstant { get; } = 1.15f;

        [SerializeField] private Image artworkImage;
        [SerializeField] private TMP_Text monthText;
        [SerializeField] private TMP_Text typeText;
        [SerializeField] private Image textBorder;
        [SerializeField] private RectTransform hoverVisualRoot;
        private Vector3 defaultVisualScale;

        public event Action<CardView> Clicked;

        public CardInstance Card { get; private set; }
        public bool IsSelected { get; private set; }
        public bool IsInteractionEnabled { get; private set; } = true;

        private void Awake()
        {
            if (hoverVisualRoot == null)
            {
                throw new InvalidOperationException("Card hover visual root is not assigned.");
            }

            defaultVisualScale = hoverVisualRoot.localScale;
        }

        public void Bind(CardInstance card, CardData cardData)
        {
            if (card == null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            if (cardData == null)
            {
                throw new ArgumentNullException(nameof(cardData));
            }

            if (!string.Equals(card.Definition.Id, cardData.CardId, StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Card instance and card data must have the same ID.",
                    nameof(cardData));
            }

            Card = card;
            SetInteractionEnabled(true);
            SetHovered(false);
            SetSelected(false);

            if (artworkImage != null)
            {
                artworkImage.sprite = cardData.Artwork;
            }

            if (monthText != null)
            {
                monthText.text = card.Definition.Month.ToString();
            }

            if (typeText != null)
            {
                typeText.text = CardTypeDisplayName.Get(card.Definition.CardType);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Card == null || !IsInteractionEnabled)
            {
                return;
            }

            Clicked?.Invoke(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Card == null || !IsInteractionEnabled)
            {
                return;
            }

            SetHovered(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetHovered(false);
        }

        public void SetSelected(bool isSelected)
        {
            IsSelected = isSelected;
            textBorder.enabled = isSelected;
        }

        public void SetInteractionEnabled(bool isEnabled)
        {
            IsInteractionEnabled = isEnabled;
            if (!isEnabled)
            {
                SetHovered(false);
            }
        }

        private void OnDisable()
        {
            SetHovered(false);
        }

        private void SetHovered(bool isHovered)
        {
            hoverVisualRoot.localScale = defaultVisualScale * (isHovered ? ScaleConstant : 1f);
        }
    }
}
