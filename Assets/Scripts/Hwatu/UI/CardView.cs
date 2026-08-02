using System;
using Hwatu.Cards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hwatu.UI
{
    [DisallowMultipleComponent]
    public sealed class CardView : MonoBehaviour
    {
        [SerializeField] private Image artworkImage;
        [SerializeField] private TMP_Text monthText;
        [SerializeField] private TMP_Text typeText;

        public CardInstance Card { get; private set; }
        public CardData Data { get; private set; }

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

        public void Clear()
        {
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
