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
        [SerializeField] private BattleDeckController battleDeckController;
        [SerializeField] private PlayerDeckInitializer playerDeckInitializer;
        [SerializeField] private EnemyEncounterController enemyEncounterController;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerHandView playerHandView;
        [SerializeField] private PlayerActionView playerActionView;
        [SerializeField] private DeckCountView deckCountView;
        [SerializeField] private BattleSequenceView battleSequenceView;
        [SerializeField] private StoreController storeController;
        [SerializeField] private UpperUIView upperUIView;
        [SerializeField] private DefeatView defeatView;

        private readonly HandEvaluator handEvaluator = new HandEvaluator();
        private readonly HandDamageCalculator damageCalculator = new HandDamageCalculator();
        private readonly BattleStakeCalculator stakeCalculator = new BattleStakeCalculator();
        private readonly List<TurnComparison> currentTurnComparisons = new List<TurnComparison>();
        private bool turnSubmitted;
        private bool cardExchangeUsed;

        private int CurrentBattleNumber => checked(
            enemyEncounterController.CurrentEncounterIndex + 1);
        private int CurrentStake => stakeCalculator.Calculate(CurrentBattleNumber);
        private IReadOnlyList<EnemyController> Enemies =>
            enemyEncounterController.CurrentEnemies;

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
                playerActionView.RerollClicked += HandleRerollClicked;
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
                playerActionView.RerollClicked -= HandleRerollClicked;
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
            enemyEncounterController.LoadInitialEncounter();
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
            RefreshPlayerHandPresentation();
        }

        private void RefreshPlayerHandPresentation()
        {
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
            ValidateActiveEnemies();
            foreach (EnemyController enemy in Enemies)
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
            playerActionView.SetSubmitInteractable(CanSubmit(selectedCards));
            playerActionView.SetRerollState(
                CanExchange(selectedCards),
                cardExchangeUsed);
        }

        private void HandleRerollClicked()
        {
            IReadOnlyList<CardInstance> selectedCards = playerHandView.SelectedCards;
            if (!CanExchange(selectedCards))
            {
                return;
            }

            if (!battleDeckController.Deck.TryExchangeCard(selectedCards[0]))
            {
                throw new InvalidOperationException(
                    "The selected player card could not be exchanged.");
            }

            cardExchangeUsed = true;
            RefreshPlayerHandPresentation();
        }

        private void HandleSubmitClicked()
        {
            IReadOnlyList<CardInstance> selectedCards = playerHandView.SelectedCards;
            if (!CanSubmit(selectedCards))
            {
                return;
            }

            turnSubmitted = true;
            playerHandView.SetInteractionEnabled(false);
            DisableActionButtons();

            HandResult playerHand = handEvaluator.Evaluate(selectedCards[0], selectedCards[1]);
            var sequenceItems = new List<BattleSequenceItem>(Enemies.Count);
            currentTurnComparisons.Clear();
            foreach (EnemyController enemy in Enemies)
            {
                if (enemy.State.IsDefeated)
                {
                    continue;
                }

                HandResult enemyHand = enemy.CurrentHandResult;
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

        private bool CanSubmit(IReadOnlyList<CardInstance> selectedCards)
        {
            return selectedCards != null
                && selectedCards.Count == 2
                && !turnSubmitted
                && !battleSequenceView.IsPlaying;
        }

        private bool CanExchange(IReadOnlyList<CardInstance> selectedCards)
        {
            return selectedCards != null
                && selectedCards.Count == 1
                && !turnSubmitted
                && !cardExchangeUsed
                && !battleSequenceView.IsPlaying;
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
                DisableActionButtons();
                defeatView.Play();
                return;
            }

            if (AreAllEnemiesDefeated())
            {
                playerHandView.SetInteractionEnabled(false);
                DisableActionButtons();

                if (enemyEncounterController.IsLastEncounter)
                {
                    storeController.EnterRunComplete();
                    return;
                }

                upperUIView.ShowStore();
                storeController.EnterStore();
                return;
            }

            battleDeckController.Deck.DiscardHand();
            foreach (EnemyController enemy in Enemies)
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
            enemyEncounterController.LoadNextEncounter();
            currentTurnComparisons.Clear();
            turnSubmitted = false;
            cardExchangeUsed = false;

            playerHandView.Clear();
            DisableActionButtons();

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
            upperUIView.ShowBattle(CurrentBattleNumber, CurrentStake);
            upperUIView.ShowMoney(playerController.State.Money);
        }

        private void DisableActionButtons()
        {
            playerActionView.SetSubmitInteractable(false);
            playerActionView.SetRerollState(
                isInteractable: false,
                isUsed: cardExchangeUsed);
        }

        private bool AreAllEnemiesDefeated()
        {
            foreach (EnemyController enemy in Enemies)
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

            if (defeatView == null)
            {
                throw new InvalidOperationException("Defeat view is not assigned.");
            }

            if (enemyEncounterController == null)
            {
                throw new InvalidOperationException(
                    "Enemy encounter controller is not assigned.");
            }
        }

        private void ValidateActiveEnemies()
        {
            IReadOnlyList<EnemyController> enemies = Enemies;
            if (enemies == null
                || enemies.Count < EnemyEncounterController.MinimumEnemyCount
                || enemies.Count > EnemyEncounterController.MaximumEnemyCount)
            {
                throw new InvalidOperationException(
                    $"Battle must contain between {EnemyEncounterController.MinimumEnemyCount} and {EnemyEncounterController.MaximumEnemyCount} active enemies.");
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
