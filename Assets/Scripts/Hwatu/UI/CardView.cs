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

        public CardData Data { get; private set; }

        public void Bind(CardData cardData)
        {
            Data = cardData;

            if (cardData == null)
            {
                Clear();
                return;
            }

            if (artworkImage != null)
            {
                artworkImage.sprite = cardData.Artwork;
            }

            if (monthText != null)
            {
                monthText.text = cardData.Month.ToString();
            }

            if (typeText != null)
            {
                typeText.text = CardTypeDisplayName.Get(cardData.CardType);
            }
        }

        public void Clear()
        {
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
