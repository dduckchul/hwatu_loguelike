using System;
using System.Collections.Generic;
using Hwatu.Cards;
using Hwatu.Deck;
using Hwatu.Hands;
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
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerHandView playerHandView;
        [SerializeField] private PlayerActionView playerActionView;
        [SerializeField] private DeckCountView deckCountView;
        [SerializeField] private List<EnemyController> enemies = new List<EnemyController>();

        private readonly HandEvaluator handEvaluator = new HandEvaluator();

        private void OnEnable()
        {
            if (playerHandView != null)
            {
                playerHandView.SelectionChanged += HandleSelectionChanged;
            }
        }

        private void OnDisable()
        {
            if (playerHandView != null)
            {
                playerHandView.SelectionChanged -= HandleSelectionChanged;
            }
        }

        private void Start()
        {
            ValidateReferences();
            InitializeEnemies();
            DrawOpeningHandCore();
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
            deckCountView.Refresh(battleDeckController.Deck);
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
            playerActionView.SetSubmitInteractable(selectedCards.Count == 2);
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

            if (playerController == null)
            {
                throw new InvalidOperationException("Player controller is not assigned.");
            }

            if (playerActionView == null)
            {
                throw new InvalidOperationException("Player action view is not assigned.");
            }

            if (deckCountView == null)
            {
                throw new InvalidOperationException("Deck count view is not assigned.");
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
