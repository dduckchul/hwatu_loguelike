using System;
using Hwatu.Deck;
using UnityEngine;
using UnityEngine.UI;

namespace Hwatu.UI
{
    [DisallowMultipleComponent]
    public sealed class CardCollectionController : MonoBehaviour
    {
        [SerializeField] private PlayerDeckInitializer playerDeckInitializer;
        [SerializeField] private BattleDeckController battleDeckController;
        [SerializeField] private CardCollectionView cardCollectionView;
        [SerializeField] private Button playerDeckButton;
        [SerializeField] private Button drawPileButton;
        [SerializeField] private Button discardPileButton;

        private void Awake()
        {
            ValidateReferences();
            cardCollectionView.Hide();
        }

        private void OnEnable()
        {
            if (playerDeckButton != null)
            {
                playerDeckButton.onClick.AddListener(ShowPlayerDeck);
            }

            if (drawPileButton != null)
            {
                drawPileButton.onClick.AddListener(ShowDrawPile);
            }

            if (discardPileButton != null)
            {
                discardPileButton.onClick.AddListener(ShowDiscardPile);
            }
        }

        private void OnDisable()
        {
            if (playerDeckButton != null)
            {
                playerDeckButton.onClick.RemoveListener(ShowPlayerDeck);
            }

            if (drawPileButton != null)
            {
                drawPileButton.onClick.RemoveListener(ShowDrawPile);
            }

            if (discardPileButton != null)
            {
                discardPileButton.onClick.RemoveListener(ShowDiscardPile);
            }
        }

        public void ShowPlayerDeck()
        {
            ValidatePlayerDeck();
            cardCollectionView.Show(
                "현재 덱",
                playerDeckInitializer.Deck.Cards,
                CardCollectionMode.Browse);
        }

        public void ShowDrawPile()
        {
            ValidateBattleDeck();
            cardCollectionView.Show(
                "잔여 패",
                battleDeckController.Deck.DrawPile,
                CardCollectionMode.Browse);
        }

        public void ShowDiscardPile()
        {
            ValidateBattleDeck();
            cardCollectionView.Show(
                "사용한 패",
                battleDeckController.Deck.DiscardPile,
                CardCollectionMode.Browse);
        }

        public void Close()
        {
            cardCollectionView.Hide();
        }

        private void ValidatePlayerDeck()
        {
            if (playerDeckInitializer.Deck == null)
            {
                throw new InvalidOperationException("Player deck is not initialized.");
            }
        }

        private void ValidateBattleDeck()
        {
            if (!battleDeckController.IsInitialized)
            {
                throw new InvalidOperationException("Battle deck is not initialized.");
            }
        }

        private void ValidateReferences()
        {
            if (playerDeckInitializer == null)
            {
                throw new InvalidOperationException(
                    "Player deck initializer is not assigned.");
            }

            if (battleDeckController == null)
            {
                throw new InvalidOperationException(
                    "Battle deck controller is not assigned.");
            }

            if (cardCollectionView == null)
            {
                throw new InvalidOperationException(
                    "Card collection view is not assigned.");
            }

            if (playerDeckButton == null)
            {
                throw new InvalidOperationException(
                    "Player deck button is not assigned.");
            }

            if (drawPileButton == null)
            {
                throw new InvalidOperationException(
                    "Draw pile button is not assigned.");
            }

            if (discardPileButton == null)
            {
                throw new InvalidOperationException(
                    "Discard pile button is not assigned.");
            }
        }
    }
}
