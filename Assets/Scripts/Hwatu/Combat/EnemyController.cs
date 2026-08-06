using System;
using System.Collections.Generic;
using Hwatu.Cards;
using Hwatu.Hands;
using Hwatu.UI;
using UnityEngine;

namespace Hwatu.Combat
{
    [DisallowMultipleComponent]
    public sealed class EnemyController : MonoBehaviour
    {
        [SerializeField] private EnemyPatternData enemyPattern;
        [SerializeField] private EnemyHandView handView;
        [SerializeField] private CharacterBattleView battleView;
        [SerializeField] private CharacterMoneyPileView moneyView;
        [SerializeField, Min(0)] private int startingMoney = 100;

        private readonly HandEvaluator handEvaluator = new HandEvaluator();
        private int currentPatternIndex;

        public CharacterState State { get; private set; }
        public int CurrentPatternIndex => currentPatternIndex;
        public CharacterBattleView BattleView => battleView;

        public void InitializeForBattle()
        {
            ValidateReferences();
            State = new CharacterState(startingMoney);
            currentPatternIndex = 0;
            RefreshMoneyView();
            RefreshHand();
        }

        public void RefreshMoneyView()
        {
            EnsureInitialized();
            moneyView.Show(State.Money);
        }

        public IReadOnlyList<CardInstance> GetCurrentCards()
        {
            EnsureInitialized();
            return enemyPattern.CreateCardsForTurn(currentPatternIndex);
        }

        public void AdvancePattern()
        {
            EnsureInitialized();
            currentPatternIndex = (currentPatternIndex + 1) % enemyPattern.PatternCount;
            RefreshHand();
        }

        private void EnsureInitialized()
        {
            if (State == null)
            {
                throw new InvalidOperationException("Enemy is not initialized for battle.");
            }
        }

        private void ValidateReferences()
        {
            if (enemyPattern == null)
            {
                throw new InvalidOperationException("Enemy pattern is not assigned.");
            }

            if (handView == null)
            {
                throw new InvalidOperationException("Enemy hand view is not assigned.");
            }

            if (battleView == null)
            {
                throw new InvalidOperationException("Enemy battle view is not assigned.");
            }

            if (moneyView == null)
            {
                throw new InvalidOperationException("Enemy money view is not assigned.");
            }

            enemyPattern.Validate();
        }

        private void RefreshHand()
        {
            IReadOnlyList<CardInstance> cards = GetCurrentCards();
            HandResult handResult = handEvaluator.Evaluate(cards[0], cards[1]);
            handView.ShowHand(
                enemyPattern.GetCardsForTurn(currentPatternIndex),
                handResult);
        }
    }
}
