using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hwatu.Combat
{
    [DisallowMultipleComponent]
    public sealed class EnemyEncounterController : MonoBehaviour
    {
        private const int MaximumEnemyCount = 2;

        [SerializeField] private StageEncounterData[] encounterSequence;
        [SerializeField] private Transform[] spawnPoints;

        private readonly List<EnemyController> currentEnemies =
            new List<EnemyController>(MaximumEnemyCount);
        private int currentEncounterIndex = -1;

        public IReadOnlyList<EnemyController> CurrentEnemies => currentEnemies;
        public int CurrentEncounterIndex => currentEncounterIndex;

        public void LoadInitialEncounter()
        {
            if (currentEncounterIndex >= 0)
            {
                throw new InvalidOperationException("The initial encounter is already loaded.");
            }

            LoadEncounter(0);
        }

        public void LoadNextEncounter()
        {
            if (currentEncounterIndex < 0)
            {
                throw new InvalidOperationException(
                    "The initial encounter must be loaded before advancing.");
            }

            LoadEncounter(checked(currentEncounterIndex + 1));
        }

        private void LoadEncounter(int encounterIndex)
        {
            ValidateConfiguration();
            if (encounterIndex < 0 || encounterIndex >= encounterSequence.Length)
            {
                throw new InvalidOperationException(
                    $"Encounter index {encounterIndex} is outside the configured sequence.");
            }

            StageEncounterData encounter = encounterSequence[encounterIndex];
            encounter.Validate();
            if (encounter.EnemyPrefabs.Count > spawnPoints.Length)
            {
                throw new InvalidOperationException(
                    $"Stage '{encounter.StageId}' has more enemies than configured spawn points.");
            }

            DestroyCurrentEnemies();

            for (int index = 0; index < encounter.EnemyPrefabs.Count; index++)
            {
                Transform spawnPoint = spawnPoints[index];
                EnemyController enemy = Instantiate(
                    encounter.EnemyPrefabs[index],
                    spawnPoint);
                enemy.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                currentEnemies.Add(enemy);
            }

            currentEncounterIndex = encounterIndex;
        }

        private void DestroyCurrentEnemies()
        {
            foreach (EnemyController enemy in currentEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy.gameObject);
                }
            }

            currentEnemies.Clear();
        }

        private void ValidateConfiguration()
        {
            if (encounterSequence == null || encounterSequence.Length == 0)
            {
                throw new InvalidOperationException(
                    "At least one stage encounter must be assigned.");
            }

            for (int index = 0; index < encounterSequence.Length; index++)
            {
                if (encounterSequence[index] == null)
                {
                    throw new InvalidOperationException(
                        $"Stage encounter at index {index} is not assigned.");
                }
            }

            if (spawnPoints == null
                || spawnPoints.Length == 0
                || spawnPoints.Length > MaximumEnemyCount)
            {
                throw new InvalidOperationException(
                    $"Between 1 and {MaximumEnemyCount} enemy spawn points must be assigned.");
            }

            var uniqueSpawnPoints = new HashSet<Transform>();
            for (int index = 0; index < spawnPoints.Length; index++)
            {
                Transform spawnPoint = spawnPoints[index];
                if (spawnPoint == null)
                {
                    throw new InvalidOperationException(
                        $"Enemy spawn point at index {index} is not assigned.");
                }

                if (!uniqueSpawnPoints.Add(spawnPoint))
                {
                    throw new InvalidOperationException(
                        "The same enemy spawn point cannot be assigned more than once.");
                }
            }
        }

    }
}
