using System;
using System.Collections.Generic;
using Hwatu.Cards;
using Hwatu.Hands;
using TMPro;
using UnityEngine;

namespace Hwatu.UI
{
    [DisallowMultipleComponent]
    public sealed class EnemyHandView : MonoBehaviour
    {
        private const int RequiredCardCount = 2;

        [SerializeField] private SpriteRenderer firstCardRenderer;
        [SerializeField] private SpriteRenderer secondCardRenderer;
        [SerializeField] private TMP_Text handNameText;

        public void ShowHand(IReadOnlyList<CardData> cards, HandResult handResult)
        {
            if (cards == null)
            {
                throw new ArgumentNullException(nameof(cards));
            }

            if (handResult == null)
            {
                throw new ArgumentNullException(nameof(handResult));
            }

            if (cards.Count != RequiredCardCount)
            {
                throw new ArgumentException(
                    $"Enemy hand must contain exactly {RequiredCardCount} cards.",
                    nameof(cards));
            }

            for (int index = 0; index < cards.Count; index++)
            {
                if (cards[index] == null)
                {
                    throw new ArgumentException(
                        $"Enemy hand card at index {index} is not assigned.",
                        nameof(cards));
                }
            }

            ValidateReferences();
            firstCardRenderer.sprite = cards[0].Artwork;
            secondCardRenderer.sprite = cards[1].Artwork;
            handNameText.text = HandDisplayName.Get(handResult);
        }

        private void ValidateReferences()
        {
            if (firstCardRenderer == null)
            {
                throw new InvalidOperationException("First enemy card renderer is not assigned.");
            }

            if (secondCardRenderer == null)
            {
                throw new InvalidOperationException("Second enemy card renderer is not assigned.");
            }

            if (handNameText == null)
            {
                throw new InvalidOperationException("Enemy hand name text is not assigned.");
            }
        }
    }
}
