using System;
using Hwatu.Cards;
using Hwatu.Combat;
using Hwatu.Deck;
using Hwatu.UI;
using UnityEngine;

namespace Hwatu.Rewards
{
    [DisallowMultipleComponent]
    public sealed class CardRemovalController : MonoBehaviour
    {
        public const int RemovalCost = 20;

        [SerializeField] private PlayerDeckInitializer playerDeckInitializer;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private StoreView storeView;
        [SerializeField] private CardCollectionView cardCollectionView;

        private bool isVisitActive;
        private bool isRemovalOpen;
        private bool removalUsed;

        public bool RemovalUsed => removalUsed;

        private void Awake()
        {
            ValidateReferences();
        }

        private void OnEnable()
        {
            storeView.RemovalRequested += HandleRemovalRequested;
            cardCollectionView.SelectionActionRequested += HandleRemovalConfirmed;
            cardCollectionView.SelectionCancelled += HandleSelectionCancelled;
            playerController.MoneyChanged += HandleMoneyChanged;
        }

        private void OnDisable()
        {
            if (storeView != null)
            {
                storeView.RemovalRequested -= HandleRemovalRequested;
            }

            if (cardCollectionView != null)
            {
                cardCollectionView.SelectionActionRequested -= HandleRemovalConfirmed;
                cardCollectionView.SelectionCancelled -= HandleSelectionCancelled;
            }

            if (playerController != null)
            {
                playerController.MoneyChanged -= HandleMoneyChanged;
            }

            Close();
        }

        public void BeginVisit()
        {
            ValidateReferences();
            if (playerDeckInitializer.Deck == null)
            {
                throw new InvalidOperationException(
                    "Player deck is not initialized.");
            }

            isVisitActive = true;
            removalUsed = false;
            CloseRemovalScreen();
            RefreshStoreButton();
        }

        public void Close()
        {
            isVisitActive = false;
            CloseRemovalScreen();
        }

        private void HandleRemovalRequested()
        {
            if (!CanStartRemoval())
            {
                return;
            }

            PlayerDeck deck = playerDeckInitializer.Deck;
            isRemovalOpen = true;
            cardCollectionView.ShowSelection(
                "버릴 패 선택",
                deck.Cards,
                deck.Cards,
                "버리기");
        }

        private void HandleRemovalConfirmed(CardInstance card)
        {
            if (!isRemovalOpen
                || removalUsed
                || !ContainsCardReference(card))
            {
                return;
            }

            if (!playerController.TrySpendMoney(RemovalCost))
            {
                RefreshStoreButton();
                return;
            }

            playerDeckInitializer.Deck.RemoveCard(card);
            removalUsed = true;
            CloseRemovalScreen();
            RefreshStoreButton();
        }

        private void HandleSelectionCancelled()
        {
            if (isRemovalOpen)
            {
                CloseRemovalScreen();
            }
        }

        private void HandleMoneyChanged(int money)
        {
            RefreshStoreButton();
        }

        private bool CanStartRemoval()
        {
            PlayerDeck deck = playerDeckInitializer.Deck;
            return isVisitActive
                && !removalUsed
                && deck != null
                && deck.CardCount > 0
                && playerController.IsInitialized
                && playerController.State.Money >= RemovalCost;
        }

        private void RefreshStoreButton()
        {
            if (storeView == null)
            {
                return;
            }

            storeView.SetRemovalState(
                RemovalCost,
                CanStartRemoval(),
                removalUsed);
        }

        private bool ContainsCardReference(CardInstance card)
        {
            if (card == null || playerDeckInitializer.Deck == null)
            {
                return false;
            }

            foreach (CardInstance deckCard in playerDeckInitializer.Deck.Cards)
            {
                if (ReferenceEquals(deckCard, card))
                {
                    return true;
                }
            }

            return false;
        }

        private void CloseRemovalScreen()
        {
            if (isRemovalOpen && cardCollectionView != null)
            {
                cardCollectionView.Hide();
            }

            isRemovalOpen = false;
        }

        private void ValidateReferences()
        {
            if (playerDeckInitializer == null
                || playerController == null
                || storeView == null
                || cardCollectionView == null)
            {
                throw new InvalidOperationException(
                    "Card removal controller references are not fully assigned.");
            }
        }
    }
}
