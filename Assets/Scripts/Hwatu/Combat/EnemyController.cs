using System;
using System.Collections.Generic;
using Hwatu.Cards;
using UnityEngine;

namespace Hwatu.Combat
{
    [DisallowMultipleComponent]
    public sealed class EnemyController : MonoBehaviour
    {
        [SerializeField] private EnemyPatternData enemyPattern;
        [SerializeField, Min(0)] private int startingMoney = 100;

        private int currentPatternIndex;

        public CharacterState State { get; private set; }
        public int CurrentPatternIndex => currentPatternIndex;

        public void InitializeForBattle()
        {
            ValidateReferences();
            State = new CharacterState(startingMoney);
            currentPatternIndex = 0;
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

            enemyPattern.Validate();
        }
    }
}
