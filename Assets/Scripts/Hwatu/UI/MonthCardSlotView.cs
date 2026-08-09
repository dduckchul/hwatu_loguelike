using System;
using Hwatu.Cards;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hwatu.UI
{
    [DisallowMultipleComponent]
    public sealed class MonthCardSlotView : MonoBehaviour
    {
        [SerializeField] private TMP_Text monthText;
        [SerializeField] private RectTransform monthCardsRoot;

        public RectTransform CardRoot => monthCardsRoot;

        public void SetMonth(int month)
        {
            ValidateReferences();
            if (month < CardDefinition.MinMonth
                || month > CardDefinition.MaxMonth)
            {
                throw new ArgumentOutOfRangeException(nameof(month));
            }

            monthText.text = $"{month}월";
        }

        private void Reset()
        {
            monthText = GetComponentInChildren<TMP_Text>(includeInactive: true);

            HorizontalLayoutGroup cardsLayout =
                GetComponentInChildren<HorizontalLayoutGroup>(includeInactive: true);
            monthCardsRoot = cardsLayout == null
                ? null
                : cardsLayout.transform as RectTransform;
        }

        private void ValidateReferences()
        {
            if (monthText == null)
            {
                throw new InvalidOperationException("Month text is not assigned.");
            }

            if (monthCardsRoot == null)
            {
                throw new InvalidOperationException("Month cards root is not assigned.");
            }
        }
    }
}
