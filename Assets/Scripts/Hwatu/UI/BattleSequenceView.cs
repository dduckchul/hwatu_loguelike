using System;
using System.Collections;
using System.Collections.Generic;
using Hwatu.Hands;
using UnityEngine;

namespace Hwatu.UI
{
    public sealed class BattleSequenceItem
    {
        public CharacterBattleView EnemyView { get; }
        public HandComparisonResult Result { get; }

        public BattleSequenceItem(
            CharacterBattleView enemyView,
            HandComparisonResult result)
        {
            EnemyView = enemyView ?? throw new ArgumentNullException(nameof(enemyView));
            Result = result;
        }
    }

    [DisallowMultipleComponent]
    public sealed class BattleSequenceView : MonoBehaviour
    {
        [SerializeField] private BattleResultView battleResultView;
        [SerializeField, Min(0f)] private float showdownDuration = 0.35f;
        [SerializeField, Min(0f)] private float resultDisplayDuration = 1f;

        private readonly List<BattleSequenceItem> sequenceItems = new List<BattleSequenceItem>();
        private CharacterBattleView playerView;
        private Coroutine sequence;

        public event Action SequenceCompleted;
        public bool IsPlaying => sequence != null;

        public void Play(
            CharacterBattleView playerBattleView,
            IReadOnlyList<BattleSequenceItem> items)
        {
            if (IsPlaying)
            {
                throw new InvalidOperationException("Battle sequence is already playing.");
            }

            if (playerBattleView == null)
            {
                throw new ArgumentNullException(nameof(playerBattleView));
            }

            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (items.Count == 0)
            {
                throw new ArgumentException("Battle sequence requires at least one enemy result.", nameof(items));
            }

            ValidateReferences();
            playerView = playerBattleView;
            sequenceItems.Clear();

            for (int index = 0; index < items.Count; index++)
            {
                BattleSequenceItem item = items[index];
                if (item == null)
                {
                    throw new ArgumentException(
                        $"Battle sequence item at index {index} is null.",
                        nameof(items));
                }

                sequenceItems.Add(item);
            }

            sequence = StartCoroutine(PlayCore());
        }

        private IEnumerator PlayCore()
        {
            playerView.ShowShowdown();
            foreach (BattleSequenceItem item in sequenceItems)
            {
                item.EnemyView.ShowShowdown();
            }

            if (showdownDuration > 0f)
            {
                yield return new WaitForSeconds(showdownDuration);
            }

            for (int index = 0; index < sequenceItems.Count; index++)
            {
                BattleSequenceItem item = sequenceItems[index];
                battleResultView.ShowPlayerResult(item.Result, index, sequenceItems.Count);
                yield return PlayResultMotion(item);

                if (index < sequenceItems.Count - 1)
                {
                    if (resultDisplayDuration > 0f)
                    {
                        yield return new WaitForSeconds(resultDisplayDuration);
                    }

                    battleResultView.Hide();
                }
            }

            ResetCharactersToIdle();
            sequence = null;
            SequenceCompleted?.Invoke();
        }

        private IEnumerator PlayResultMotion(BattleSequenceItem item)
        {
            switch (item.Result)
            {
                case HandComparisonResult.FirstWins:
                    yield return PlayAttackAndHit(playerView, item.EnemyView);
                    break;
                case HandComparisonResult.SecondWins:
                    yield return PlayAttackAndHit(item.EnemyView, playerView);
                    break;
                case HandComparisonResult.Draw:
                    yield break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(item.Result), item.Result, null);
            }
        }

        private static IEnumerator PlayAttackAndHit(
            CharacterBattleView attacker,
            CharacterBattleView defender)
        {
            yield return attacker.PlayAttackForward();
            yield return defender.PlayHit();
            yield return attacker.PlayAttackReturn();
        }

        private void ResetCharactersToIdle()
        {
            if (playerView != null)
            {
                playerView.ShowIdle();
            }

            foreach (BattleSequenceItem item in sequenceItems)
            {
                item.EnemyView.ShowIdle();
            }
        }

        private void OnDisable()
        {
            if (sequence == null)
            {
                return;
            }

            StopCoroutine(sequence);
            sequence = null;
            ResetCharactersToIdle();
        }

        private void ValidateReferences()
        {
            if (battleResultView == null)
            {
                throw new InvalidOperationException("Battle result view is not assigned.");
            }
        }
    }
}
