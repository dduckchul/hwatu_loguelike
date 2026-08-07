using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hwatu.Combat
{
    [CreateAssetMenu(
        fileName = "StageEncounterData",
        menuName = "Hwatu/Combat/Stage Encounter")]
    public sealed class StageEncounterData : ScriptableObject
    {
        private const int MinimumEnemyCount = 1;
        private const int MaximumEnemyCount = 2;

        [SerializeField] private string stageId;
        [SerializeField] private EnemyController[] enemyPrefabs;

        public string StageId => stageId;
        public IReadOnlyList<EnemyController> EnemyPrefabs => enemyPrefabs;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(stageId))
            {
                throw new InvalidOperationException("Stage encounter ID must not be empty.");
            }

            if (enemyPrefabs == null
                || enemyPrefabs.Length < MinimumEnemyCount
                || enemyPrefabs.Length > MaximumEnemyCount)
            {
                throw new InvalidOperationException(
                    $"Stage '{stageId}' must contain between {MinimumEnemyCount} and {MaximumEnemyCount} enemy prefabs.");
            }

            for (int index = 0; index < enemyPrefabs.Length; index++)
            {
                EnemyController enemyPrefab = enemyPrefabs[index];
                if (enemyPrefab == null)
                {
                    throw new InvalidOperationException(
                        $"Enemy prefab at index {index} in stage '{stageId}' is not assigned.");
                }
            }
        }
    }
}
