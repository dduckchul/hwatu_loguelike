using System;
using System.Collections.Generic;
using Hwatu.Cards;
using Hwatu.Deck;
using Hwatu.Hands;
using Hwatu.Rewards;
using Hwatu.UI;
using UnityEngine;

namespace Hwatu.Combat
{
    [DisallowMultipleComponent]
    public sealed class BattleController : MonoBehaviour
    {
        private const int MinimumEnemyCount = 1;
        private const int MaximumEnemyCount = 2;

        [SerializeField] private BattleDeckController battleDeckController;
        [SerializeField] private PlayerDeckInitializer playerDeckInitializer;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerHandView playerHandView;
        [SerializeField] private PlayerActionView playerActionView;
        [SerializeField] private DeckCountView deckCountView;
        [SerializeField] private BattleSequenceView battleSequenceView;
        [SerializeField] private StoreController storeController;
        [SerializeField] private UpperUIView upperUIView;
        [SerializeField, Min(1)] private int currentBattleNumber = 1;
        [SerializeField] private List<EnemyController> enemies = new List<EnemyController>();

        private readonly HandEvaluator handEvaluator = new HandEvaluator();
        private readonly HandDamageCalculator damageCalculator = new HandDamageCalculator();
        private readonly BattleStakeCalculator stakeCalculator = new BattleStakeCalculator();
        private readonly List<TurnComparison> currentTurnComparisons = new List<TurnComparison>();
        private bool turnSubmitted;

        private int CurrentStake => stakeCalculator.Calculate(currentBattleNumber);

        private sealed class TurnComparison
        {
            public EnemyController Enemy { get; }
            public HandResult PlayerHand { get; }
            public HandResult EnemyHand { get; }
            public HandComparisonResult Result { get; }

            public TurnComparison(
                EnemyController enemy,
                HandResult playerHand,
                HandResult enemyHand,
                HandComparisonResult result)
            {
                Enemy = enemy;
                PlayerHand = playerHand;
                EnemyHand = enemyHand;
                Result = result;
            }
        }

        private void OnEnable()
        {
            if (playerHandView != null)
            {
                playerHandView.SelectionChanged += HandleSelectionChanged;
            }

            if (playerActionView != null)
            {
                playerActionView.SubmitClicked += HandleSubmitClicked;
            }

            if (playerController != null)
            {
                playerController.MoneyChanged += HandlePlayerMoneyChanged;
            }

            if (battleSequenceView != null)
            {
                battleSequenceView.ResultMotionCompleted += HandleResultMotionCompleted;
                battleSequenceView.SequenceCompleted += HandleSequenceCompleted;
            }

            if (storeController != null)
            {
                storeController.NextBattlePreparationRequested +=
                    HandleNextBattlePreparationRequested;
                storeController.BattlePresentationRestored += HandleBattlePresentationRestored;
            }
        }

        private void OnDisable()
        {
            if (playerHandView != null)
            {
                playerHandView.SelectionChanged -= HandleSelectionChanged;
            }

            if (playerActionView != null)
            {
                playerActionView.SubmitClicked -= HandleSubmitClicked;
            }

            if (playerController != null)
            {
                playerController.MoneyChanged -= HandlePlayerMoneyChanged;
            }

            if (battleSequenceView != null)
            {
                battleSequenceView.ResultMotionCompleted -= HandleResultMotionCompleted;
                battleSequenceView.SequenceCompleted -= HandleSequenceCompleted;
            }

            if (storeController != null)
            {
                storeController.NextBattlePreparationRequested -=
                    HandleNextBattlePreparationRequested;
                storeController.BattlePresentationRestored -= HandleBattlePresentationRestored;
            }
        }

        private void Start()
        {
            ValidateReferences();
            InitializePlayer();
            InitializeEnemies();
            RefreshUpperUiForBattle();
            DrawOpeningHandCore();
        }

        private void HandlePlayerMoneyChanged(int money)
        {
            if (upperUIView != null)
            {
                upperUIView.ShowMoney(money);
            }
        }

        public void DrawOpeningHand()
        {
            ValidateReferences();
            DrawOpeningHandCore();
        }

        private void DrawOpeningHandCore()
        {
            if (!battleDeckController.IsInitialized)
            {
                throw new InvalidOperationException("Battle deck is not initialized.");
            }

            battleDeckController.Deck.DrawToHand();
            playerHandView.SetCards(battleDeckController.Deck.Hand);
            playerHandView.SetInteractionEnabled(true);
            deckCountView.Refresh(battleDeckController.Deck);
        }

        private void InitializePlayer()
        {
            if (!playerController.IsInitialized)
            {
                playerController.InitializeForRun();
            }
        }

        private void InitializeEnemies()
        {
            foreach (EnemyController enemy in enemies)
            {
                enemy.InitializeForBattle();
            }
        }

        private void HandleSelectionChanged(IReadOnlyList<CardInstance> selectedCards)
        {
            if (selectedCards == null)
            {
                throw new ArgumentNullException(nameof(selectedCards));
            }

            HandResult handResult = selectedCards.Count == 2
                ? handEvaluator.Evaluate(selectedCards[0], selectedCards[1])
                : null;

            playerHandView.RefreshSelectionDisplay(handResult);
            playerActionView.SetSubmitInteractable(
                !turnSubmitted && selectedCards.Count == 2);
        }

        private void HandleSubmitClicked()
        {
            IReadOnlyList<CardInstance> selectedCards = playerHandView.SelectedCards;
            if (selectedCards.Count != 2)
            {
                throw new InvalidOperationException("Exactly two player cards must be selected before submitting.");
            }

            if (turnSubmitted || battleSequenceView.IsPlaying)
            {
                return;
            }

            turnSubmitted = true;
            playerHandView.SetInteractionEnabled(false);
            playerActionView.SetSubmitInteractable(false);

            HandResult playerHand = handEvaluator.Evaluate(selectedCards[0], selectedCards[1]);
            var sequenceItems = new List<BattleSequenceItem>(enemies.Count);
            currentTurnComparisons.Clear();
            foreach (EnemyController enemy in enemies)
            {
                if (enemy.State.IsDefeated)
                {
                    continue;
                }

                IReadOnlyList<CardInstance> enemyCards = enemy.GetCurrentCards();
                HandResult enemyHand = handEvaluator.Evaluate(enemyCards[0], enemyCards[1]);
                HandComparisonResult result = HandComparer.Compare(playerHand, enemyHand);
                currentTurnComparisons.Add(
                    new TurnComparison(enemy, playerHand, enemyHand, result));
                sequenceItems.Add(new BattleSequenceItem(enemy.BattleView, result));
            }

            if (sequenceItems.Count == 0)
            {
                throw new InvalidOperationException("Cannot submit a turn without an active enemy.");
            }

            battleSequenceView.Play(playerController.BattleView, sequenceItems);
        }

        private void HandleResultMotionCompleted(int resultIndex)
        {
            if (resultIndex < 0 || resultIndex >= currentTurnComparisons.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(resultIndex));
            }

            TurnComparison comparison = currentTurnComparisons[resultIndex];
            if (playerController.State.IsDefeated || comparison.Enemy.State.IsDefeated)
            {
                return;
            }

            switch (comparison.Result)
            {
                case HandComparisonResult.FirstWins:
                    comparison.Enemy.State.TransferMoneyTo(
                        playerController.State,
                        damageCalculator.Calculate(comparison.PlayerHand, CurrentStake));
                    break;
                case HandComparisonResult.SecondWins:
                    playerController.State.TransferMoneyTo(
                        comparison.Enemy.State,
                        damageCalculator.Calculate(comparison.EnemyHand, CurrentStake));
                    break;
                case HandComparisonResult.Draw:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(comparison.Result),
                        comparison.Result,
                        null);
            }

            playerController.RefreshMoneyView();
            upperUIView.ShowMoney(playerController.State.Money);
            comparison.Enemy.RefreshMoneyView();

            if (playerController.State.IsDefeated)
            {
                battleSequenceView.StopAfterCurrentResult();
            }
        }

        private void HandleSequenceCompleted()
        {
            currentTurnComparisons.Clear();

            if (playerController.State.IsDefeated)
            {
                playerHandView.SetInteractionEnabled(false);
                playerActionView.SetSubmitInteractable(false);
                return;
            }

            if (AreAllEnemiesDefeated())
            {
                playerHandView.SetInteractionEnabled(false);
                playerActionView.SetSubmitInteractable(false);
                upperUIView.ShowStore();
                storeController.EnterStore();
                return;
            }

            battleDeckController.Deck.DiscardHand();
            foreach (EnemyController enemy in enemies)
            {
                if (!enemy.State.IsDefeated)
                {
                    enemy.AdvancePattern();
                }
            }

            turnSubmitted = false;
            DrawOpeningHandCore();
        }

        private void HandleNextBattlePreparationRequested()
        {
            ValidateReferences();
            currentBattleNumber = checked(currentBattleNumber + 1);
            currentTurnComparisons.Clear();
            turnSubmitted = false;

            playerHandView.Clear();
            playerActionView.SetSubmitInteractable(false);
            playerActionView.SetRerollInteractable(false);

            playerDeckInitializer.RebuildBattleDeck();
            InitializeEnemies();
            RefreshUpperUiForBattle();
            deckCountView.Refresh(battleDeckController.Deck);
        }

        private void HandleBattlePresentationRestored()
        {
            DrawOpeningHandCore();
        }

        private void RefreshUpperUiForBattle()
        {
            upperUIView.ShowBattle(currentBattleNumber, CurrentStake);
            upperUIView.ShowMoney(playerController.State.Money);
        }

        private bool AreAllEnemiesDefeated()
        {
            foreach (EnemyController enemy in enemies)
            {
                if (!enemy.State.IsDefeated)
                {
                    return false;
                }
            }

            return true;
        }

        private void ValidateReferences()
        {
            if (battleDeckController == null)
            {
                throw new InvalidOperationException("Battle deck controller is not assigned.");
            }

            if (playerDeckInitializer == null)
            {
                throw new InvalidOperationException(
                    "Player deck initializer is not assigned.");
            }

            if (playerHandView == null)
            {
                throw new InvalidOperationException("Player hand view is not assigned.");
            }

            if (playerController == null)
            {
                throw new InvalidOperationException("Player controller is not assigned.");
            }

            if (playerController.BattleView == null)
            {
                throw new InvalidOperationException("Player battle view is not assigned.");
            }

            if (playerActionView == null)
            {
                throw new InvalidOperationException("Player action view is not assigned.");
            }

            if (deckCountView == null)
            {
                throw new InvalidOperationException("Deck count view is not assigned.");
            }

            if (battleSequenceView == null)
            {
                throw new InvalidOperationException("Battle sequence view is not assigned.");
            }

            if (storeController == null)
            {
                throw new InvalidOperationException("Store controller is not assigned.");
            }

            if (upperUIView == null)
            {
                throw new InvalidOperationException("Upper UI view is not assigned.");
            }

            if (enemies == null
                || enemies.Count < MinimumEnemyCount
                || enemies.Count > MaximumEnemyCount)
            {
                throw new InvalidOperationException(
                    $"Battle must contain between {MinimumEnemyCount} and {MaximumEnemyCount} enemies.");
            }

            var registeredEnemies = new HashSet<EnemyController>();
            for (int index = 0; index < enemies.Count; index++)
            {
                EnemyController enemy = enemies[index];
                if (enemy == null)
                {
                    throw new InvalidOperationException($"Enemy at index {index} is not assigned.");
                }

                if (!registeredEnemies.Add(enemy))
                {
                    throw new InvalidOperationException("The same enemy cannot be registered more than once.");
                }
            }
        }
    }
}
