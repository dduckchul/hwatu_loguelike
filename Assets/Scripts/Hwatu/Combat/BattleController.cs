using System;
using Hwatu.Deck;
using Hwatu.UI;
using UnityEngine;

namespace Hwatu.Combat
{
    [DisallowMultipleComponent]
    public sealed class BattleController : MonoBehaviour
    {
        [SerializeField] private BattleDeckController battleDeckController;
        [SerializeField] private PlayerHandView playerHandView;
        [SerializeField] private DeckCountView deckCountView;

        private void Start()
        {
            DrawOpeningHand();
        }

        public void DrawOpeningHand()
        {
            ValidateReferences();

            if (!battleDeckController.IsInitialized)
            {
                throw new InvalidOperationException("Battle deck is not initialized.");
            }

            battleDeckController.Deck.DrawToHand();
            playerHandView.SetCards(battleDeckController.Deck.Hand);
            deckCountView.Refresh(battleDeckController.Deck);
        }

        private void ValidateReferences()
        {
            if (battleDeckController == null)
            {
                throw new InvalidOperationException("Battle deck controller is not assigned.");
            }

            if (playerHandView == null)
            {
                throw new InvalidOperationException("Player hand view is not assigned.");
            }

            if (deckCountView == null)
            {
                throw new InvalidOperationException("Deck count view is not assigned.");
            }
        }
    }
}
