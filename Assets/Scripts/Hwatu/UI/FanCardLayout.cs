using System.Collections.Generic;
using UnityEngine;

namespace Hwatu.UI
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class FanCardLayout : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float cardSpacing = 120f;
        [SerializeField, Min(0f)] private float arcHeight = 30f;
        [SerializeField, Range(0f, 90f)] private float maxRotationAngle = 15f;

        private readonly List<RectTransform> cards = new List<RectTransform>();

        public void RefreshLayout()
        {
            CollectActiveCards();

            int cardCount = cards.Count;
            if (cardCount == 0)
            {
                return;
            }

            if (cardCount == 1)
            {
                SetCardTransform(cards[0], Vector2.zero, 0f);
                return;
            }

            float centerIndex = (cardCount - 1) * 0.5f;
            for (int index = 0; index < cardCount; index++)
            {
                float normalizedPosition = (index - centerIndex) / centerIndex;
                float x = (index - centerIndex) * cardSpacing;
                float y = -arcHeight * normalizedPosition * normalizedPosition;
                float rotation = -maxRotationAngle * normalizedPosition;

                SetCardTransform(cards[index], new Vector2(x, y), rotation);
            }
        }

        private void OnEnable()
        {
            RefreshLayout();
        }

        private void OnValidate()
        {
            RefreshLayout();
        }

        private void OnTransformChildrenChanged()
        {
            RefreshLayout();
        }

        private void OnRectTransformDimensionsChange()
        {
            RefreshLayout();
        }

        private void CollectActiveCards()
        {
            cards.Clear();

            for (int index = 0; index < transform.childCount; index++)
            {
                Transform child = transform.GetChild(index);
                if (!child.gameObject.activeSelf)
                {
                    continue;
                }

                RectTransform card = child as RectTransform;
                if (card != null)
                {
                    cards.Add(card);
                }
            }
        }

        private static void SetCardTransform(
            RectTransform card,
            Vector2 anchoredPosition,
            float rotation)
        {
            card.anchorMin = new Vector2(0.5f, 0.5f);
            card.anchorMax = new Vector2(0.5f, 0.5f);
            card.anchoredPosition = anchoredPosition;
            card.localRotation = Quaternion.Euler(0f, 0f, rotation);
        }
    }
}
