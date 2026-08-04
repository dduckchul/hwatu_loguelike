using System;
using Hwatu.Cards;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hwatu.UI
{
    [DisallowMultipleComponent]
    public sealed class CardView : MonoBehaviour, IPointerClickHandler
    {
        public const float SelectedScale = 1.15f;

        [SerializeField] private Image artworkImage;
        [SerializeField] private TMP_Text monthText;
        [SerializeField] private TMP_Text typeText;
        [SerializeField] private Image textBorder;
        public event Action<CardView> Clicked;

        public CardInstance Card { get; private set; }
        public CardData Data { get; private set; }
        public bool IsSelected { get; private set; }

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
            Data = cardData;
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

        public void SetSelected(bool isSelected)
        {
            IsSelected = isSelected;
            transform.localScale = Vector3.one * (isSelected ? SelectedScale : 1f);
            textBorder.enabled = isSelected;
        }

        public void Clear()
        {
            SetSelected(false);
            Card = null;
            Data = null;

            if (artworkImage != null)
            {
                artworkImage.sprite = null;
            }

            if (monthText != null)
            {
                monthText.text = string.Empty;
            }

            if (typeText != null)
            {
                typeText.text = string.Empty;
            }
        }
    }
}
