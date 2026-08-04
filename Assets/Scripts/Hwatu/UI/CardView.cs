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
        private Vector3 defaultScale;

        public event Action<CardView> Clicked;

        public CardInstance Card { get; private set; }
        public bool IsSelected { get; private set; }

        private void Awake()
        {
            defaultScale = transform.localScale;
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
            if (Card == null)
            {
                return;
            }

            Clicked?.Invoke(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Card == null)
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

        private void OnDisable()
        {
            SetHovered(false);
        }

        private void SetHovered(bool isHovered)
        {
            transform.localScale = defaultScale * (isHovered ? ScaleConstant : 1f);
        }
    }
}
