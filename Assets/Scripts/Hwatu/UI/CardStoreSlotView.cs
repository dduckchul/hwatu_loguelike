using System;
using TMPro;
using UnityEngine;

namespace Hwatu.UI
{
    [DisallowMultipleComponent]
    public sealed class CardStoreSlotView : MonoBehaviour
    {
        [SerializeField] private RectTransform cardRoot;
        [SerializeField] private TMP_Text cardCostText;
        [SerializeField] private GameObject soldOutView;

        public RectTransform CardRoot
        {
            get
            {
                if (cardRoot == null)
                {
                    throw new InvalidOperationException(
                        "Card root is not assigned to the card store slot.");
                }

                return cardRoot;
            }
        }

        public void ResetSlot(int cardCost)
        {
            ValidateReferences();
            SetCost(cardCost);
            soldOutView.SetActive(false);
        }

        public void SetCost(int cardCost)
        {
            if (cardCostText == null)
            {
                throw new InvalidOperationException(
                    "Card cost text is not assigned to the card store slot.");
            }

            if (cardCost < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cardCost),
                    "Card cost cannot be negative.");
            }

            cardCostText.text = $"{cardCost} 전";
        }

        public void MarkSoldOut()
        {
            if (soldOutView == null)
            {
                throw new InvalidOperationException(
                    "Sold out view is not assigned to the card store slot.");
            }

            soldOutView.SetActive(true);
        }

        private void ValidateReferences()
        {
            if (cardRoot == null)
            {
                throw new InvalidOperationException(
                    "Card root is not assigned to the card store slot.");
            }

            if (cardCostText == null)
            {
                throw new InvalidOperationException(
                    "Card cost text is not assigned to the card store slot.");
            }

            if (soldOutView == null)
            {
                throw new InvalidOperationException(
                    "Sold out view is not assigned to the card store slot.");
            }
        }
    }
}
