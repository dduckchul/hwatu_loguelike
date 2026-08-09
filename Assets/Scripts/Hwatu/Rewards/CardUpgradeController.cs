using System;
using System.Collections.Generic;
using Hwatu.Cards;
using Hwatu.Combat;
using Hwatu.Deck;
using Hwatu.UI;
using UnityEngine;

namespace Hwatu.Rewards
{
    [DisallowMultipleComponent]
    public sealed class CardUpgradeController : MonoBehaviour
    {
        public const int UpgradeCost = 15;

        [SerializeField] private PlayerDeckInitializer playerDeckInitializer;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private CardCatalogData cardCatalog;
        [SerializeField] private StoreView storeView;
        [SerializeField] private CardCollectionView cardCollectionView;
        [SerializeField] private CardUpgradeChoiceView choiceView;

        private bool isVisitActive;
        private bool isUpgradeOpen;
        private bool upgradeUsed;
        private CardInstance selectedCard;

        public bool UpgradeUsed => upgradeUsed;

        private void Awake()
        {
            ValidateReferences();
            choiceView.Hide();
        }

        private void OnEnable()
        {
            storeView.UpgradeRequested += HandleUpgradeRequested;
            cardCollectionView.CardSelected += HandleCardSelected;
            cardCollectionView.SelectionCancelled += HandleSelectionCancelled;
            choiceView.CandidateSelected += HandleCandidateSelected;
            choiceView.BackRequested += HandleBackRequested;
            playerController.MoneyChanged += HandleMoneyChanged;
        }

        private void OnDisable()
        {
            if (storeView != null)
            {
                storeView.UpgradeRequested -= HandleUpgradeRequested;
            }

            if (cardCollectionView != null)
            {
                cardCollectionView.CardSelected -= HandleCardSelected;
                cardCollectionView.SelectionCancelled -= HandleSelectionCancelled;
            }

            if (choiceView != null)
            {
                choiceView.CandidateSelected -= HandleCandidateSelected;
                choiceView.BackRequested -= HandleBackRequested;
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
            upgradeUsed = false;
            CloseUpgradeScreen();
            RefreshStoreButton();
        }

        public void Close()
        {
            isVisitActive = false;
            CloseUpgradeScreen();
        }

        private void HandleUpgradeRequested()
        {
            if (!CanStartUpgrade())
            {
                return;
            }

            List<CardInstance> eligibleCards = GetEligibleCards();
            isUpgradeOpen = true;
            selectedCard = null;
            choiceView.Hide();
            cardCollectionView.ShowSelection(
                "강화할 패 선택",
                playerDeckInitializer.Deck.Cards,
                eligibleCards);
        }

        private void HandleCardSelected(CardInstance card)
        {
            if (!isUpgradeOpen || !IsEligible(card))
            {
                return;
            }

            selectedCard = card;
            choiceView.Show(card, GetCandidates(card));
        }

        private void HandleCandidateSelected(CardData candidate)
        {
            if (!isUpgradeOpen
                || upgradeUsed
                || selectedCard == null
                || !ContainsCardReference(selectedCard)
                || !IsCandidate(selectedCard, candidate))
            {
                return;
            }

            if (!playerController.TrySpendMoney(UpgradeCost))
            {
                RefreshStoreButton();
                return;
            }

            playerDeckInitializer.Deck.UpgradeCard(
                selectedCard,
                candidate.ToDefinition());
            upgradeUsed = true;
            CloseUpgradeScreen();
            RefreshStoreButton();
        }

        private void HandleSelectionCancelled()
        {
            if (isUpgradeOpen)
            {
                CloseUpgradeScreen();
            }
        }

        private void HandleBackRequested()
        {
            if (!isUpgradeOpen)
            {
                return;
            }

            selectedCard = null;
            choiceView.Hide();
        }

        private void HandleMoneyChanged(int money)
        {
            RefreshStoreButton();
        }

        private bool CanStartUpgrade()
        {
            return isVisitActive
                && !upgradeUsed
                && playerController.IsInitialized
                && playerController.State.Money >= UpgradeCost
                && GetEligibleCards().Count > 0;
        }

        private void RefreshStoreButton()
        {
            if (storeView == null)
            {
                return;
            }

            storeView.SetUpgradeState(
                UpgradeCost,
                CanStartUpgrade(),
                upgradeUsed);
        }

        private List<CardInstance> GetEligibleCards()
        {
            var eligibleCards = new List<CardInstance>();
            PlayerDeck deck = playerDeckInitializer.Deck;
            if (deck == null)
            {
                return eligibleCards;
            }

            foreach (CardInstance card in deck.Cards)
            {
                if (IsEligible(card))
                {
                    eligibleCards.Add(card);
                }
            }

            return eligibleCards;
        }

        private bool IsEligible(CardInstance card)
        {
            return card != null
                && card.Definition.CardType == CardType.Normal
                && GetCandidates(card).Count > 0;
        }

        private List<CardData> GetCandidates(CardInstance sourceCard)
        {
            var candidates = new List<CardData>(2);
            foreach (CardData cardData in cardCatalog.Cards)
            {
                if (cardData == null)
                {
                    throw new InvalidOperationException(
                        "Card catalog cannot contain a null card.");
                }

                if (cardData.Month == sourceCard.Definition.Month
                    && (cardData.CardType == CardType.Ribbon
                        || cardData.CardType == CardType.Animal))
                {
                    candidates.Add(cardData);
                }
            }

            candidates.Sort((first, second) =>
                first.CardType.CompareTo(second.CardType));
            return candidates;
        }

        private bool IsCandidate(CardInstance sourceCard, CardData candidate)
        {
            if (candidate == null)
            {
                return false;
            }

            foreach (CardData available in GetCandidates(sourceCard))
            {
                if (available.CardId == candidate.CardId)
                {
                    return true;
                }
            }

            return false;
        }

        private bool ContainsCardReference(CardInstance card)
        {
            foreach (CardInstance deckCard in playerDeckInitializer.Deck.Cards)
            {
                if (ReferenceEquals(deckCard, card))
                {
                    return true;
                }
            }

            return false;
        }

        private void CloseUpgradeScreen()
        {
            selectedCard = null;
            if (choiceView != null)
            {
                choiceView.Hide();
            }

            if (isUpgradeOpen && cardCollectionView != null)
            {
                cardCollectionView.Hide();
            }

            isUpgradeOpen = false;
        }

        private void ValidateReferences()
        {
            if (playerDeckInitializer == null
                || playerController == null
                || cardCatalog == null
                || storeView == null
                || cardCollectionView == null
                || choiceView == null)
            {
                throw new InvalidOperationException(
                    "Card upgrade controller references are not fully assigned.");
            }
        }
    }
}
