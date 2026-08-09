using System;
using System.Collections;
using System.Collections.Generic;
using Hwatu.Cards;
using Hwatu.Hands;
using Hwatu.UI;
using TMPro;
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

        [Header("Defeat")]
        [SerializeField, Min(0f)] private float defeatFadeDuration = 0.5f;

        private readonly HandEvaluator handEvaluator = new HandEvaluator();
        private int currentPatternIndex;
        private IReadOnlyList<CardInstance> currentCards;
        private HandResult currentHandResult;
        private Coroutine defeatFadeCoroutine;

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

        public void PlayDefeatFadeOut()
        {
            EnsureInitialized();
            if (!State.IsDefeated)
            {
                throw new InvalidOperationException(
                    "An enemy that still has money cannot play its defeat fade out.");
            }

            if (defeatFadeCoroutine == null)
            {
                defeatFadeCoroutine = StartCoroutine(PlayDefeatFadeOutCore());
            }
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

        private IEnumerator PlayDefeatFadeOutCore()
        {
            SpriteRenderer[] spriteRenderers =
                GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
            TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(includeInactive: true);
            Color[] spriteColors = CaptureColors(spriteRenderers);
            Color[] textColors = CaptureColors(texts);

            if (defeatFadeDuration > 0f)
            {
                float elapsed = 0f;
                while (elapsed < defeatFadeDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float progress = Mathf.Clamp01(elapsed / defeatFadeDuration);
                    float alphaMultiplier = 1f - Mathf.SmoothStep(0f, 1f, progress);

                    ApplyAlpha(spriteRenderers, spriteColors, alphaMultiplier);
                    ApplyAlpha(texts, textColors, alphaMultiplier);
                    yield return null;
                }
            }

            defeatFadeCoroutine = null;
            gameObject.SetActive(false);
        }

        private static Color[] CaptureColors(SpriteRenderer[] renderers)
        {
            var colors = new Color[renderers.Length];
            for (int index = 0; index < renderers.Length; index++)
            {
                colors[index] = renderers[index].color;
            }

            return colors;
        }

        private static Color[] CaptureColors(TMP_Text[] texts)
        {
            var colors = new Color[texts.Length];
            for (int index = 0; index < texts.Length; index++)
            {
                colors[index] = texts[index].color;
            }

            return colors;
        }

        private static void ApplyAlpha(
            SpriteRenderer[] renderers,
            Color[] originalColors,
            float alphaMultiplier)
        {
            for (int index = 0; index < renderers.Length; index++)
            {
                Color color = originalColors[index];
                color.a *= alphaMultiplier;
                renderers[index].color = color;
            }
        }

        private static void ApplyAlpha(
            TMP_Text[] texts,
            Color[] originalColors,
            float alphaMultiplier)
        {
            for (int index = 0; index < texts.Length; index++)
            {
                Color color = originalColors[index];
                color.a *= alphaMultiplier;
                texts[index].color = color;
            }
        }
    }
}
