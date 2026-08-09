using System;
using System.Collections.Generic;
using Hwatu.Cards;
using UnityEngine;

namespace Hwatu.Combat
{
    [Serializable]
    public sealed class EnemyTurnPattern
    {
        public const int RequiredCardCount = 2;

        [SerializeField] private List<CardData> cards = new List<CardData>(RequiredCardCount);

        public IReadOnlyList<CardData> Cards => cards;

        public void Validate()
        {
            if (cards == null || cards.Count != RequiredCardCount)
            {
                throw new InvalidOperationException(
                    $"Enemy turn pattern must contain exactly {RequiredCardCount} cards.");
            }

            for (int index = 0; index < cards.Count; index++)
            {
                if (cards[index] == null)
                {
                    throw new InvalidOperationException(
                        $"Enemy turn pattern card at index {index} is not assigned.");
                }
            }
        }
    }

    [CreateAssetMenu(fileName = "EnemyPatternData", menuName = "Hwatu/Combat/Enemy Pattern")]
    public sealed class EnemyPatternData : ScriptableObject
    {
        [SerializeField] private List<EnemyTurnPattern> patterns = new List<EnemyTurnPattern>();

        public int PatternCount => patterns == null ? 0 : patterns.Count;

        public IReadOnlyList<CardData> GetCardsForTurn(int turnIndex)
        {
            return GetPatternForTurn(turnIndex).Cards;
        }

        public void Validate()
        {
            if (patterns == null || patterns.Count == 0)
            {
                throw new InvalidOperationException("Enemy pattern must contain at least one turn pattern.");
            }

            for (int index = 0; index < patterns.Count; index++)
            {
                EnemyTurnPattern pattern = patterns[index];
                if (pattern == null)
                {
                    throw new InvalidOperationException(
                        $"Enemy turn pattern at index {index} is not assigned.");
                }

                pattern.Validate();
            }
        }

        private EnemyTurnPattern GetPatternForTurn(int turnIndex)
        {
            if (turnIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(turnIndex));
            }

            Validate();

            int patternIndex = turnIndex % patterns.Count;
            return patterns[patternIndex];
        }
    }
}
