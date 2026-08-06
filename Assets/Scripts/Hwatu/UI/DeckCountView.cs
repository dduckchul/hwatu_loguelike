using System;
using Hwatu.Deck;
using TMPro;
using UnityEngine;

namespace Hwatu.UI
{
    [DisallowMultipleComponent]
    public sealed class DeckCountView : MonoBehaviour
    {
        [SerializeField] private TMP_Text deckCountText;
        [SerializeField] private TMP_Text usedCardCountText;

        public void Refresh(BattleDeck deck)
        {
            if (deck == null)
            {
                throw new ArgumentNullException(nameof(deck));
            }

            ValidateReferences();

            deckCountText.text = $"{deck.DrawPileCount}/{deck.TotalCardCount}";
            usedCardCountText.text = deck.DiscardPileCount.ToString();
        }

        private void ValidateReferences()
        {
            if (deckCountText == null)
            {
                throw new InvalidOperationException("Deck count text is not assigned.");
            }

            if (usedCardCountText == null)
            {
                throw new InvalidOperationException("Used card count text is not assigned.");
            }
        }
    }
}
