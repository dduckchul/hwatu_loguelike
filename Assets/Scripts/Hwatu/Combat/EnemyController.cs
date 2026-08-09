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
        private IReadOnlyList<CardInstance> currentCards;
        private HandResult currentHandResult;

        public CharacterState State { get; private set; }
        public int CurrentPatternIndex => currentPatternIndex;
        public CharacterBattleView BattleView => battleView;
        public HandResult CurrentHandResult
        {
            get
            {
                EnsureInitialized();
                return currentHandResult;
            }
        }

        public void InitializeForBattle()
        {
            ValidateReferences();
            State = new CharacterState(startingMoney);
            currentPatternIndex = 0;
            battleView.ShowIdle();
            RefreshMoneyView();
            RefreshCurrentHand();
        }

        public void RefreshMoneyView()
        {
            EnsureInitialized();
            moneyView.Show(State.Money);
        }

        public IReadOnlyList<CardInstance> GetCurrentCards()
        {
            EnsureInitialized();
            return currentCards;
        }

        public void AdvancePattern()
        {
            EnsureInitialized();
            currentPatternIndex = (currentPatternIndex + 1) % enemyPattern.PatternCount;
            RefreshCurrentHand();
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

        private void RefreshCurrentHand()
        {
            IReadOnlyList<CardData> cardData =
                enemyPattern.GetCardsForTurn(currentPatternIndex);
            var cards = new List<CardInstance>(EnemyTurnPattern.RequiredCardCount);
            foreach (CardData card in cardData)
            {
                cards.Add(new CardInstance(card.ToDefinition()));
            }

            currentCards = cards.AsReadOnly();
            currentHandResult = handEvaluator.Evaluate(currentCards[0], currentCards[1]);
            handView.ShowHand(cardData, currentHandResult);
        }
    }
}
