using System;
using System.Collections;
using UnityEngine;

namespace Hwatu.UI
{
    [DisallowMultipleComponent]
    public sealed class BackgroundTransitionView : MonoBehaviour
    {
        [Header("Background Renderers")]
        [SerializeField] private SpriteRenderer battleBackgroundRenderer;
        [SerializeField] private SpriteRenderer storeBackgroundRenderer;

        [Header("Transition")]
        [SerializeField, Min(0f)] private float transitionDuration = 0.75f;
        [SerializeField] private bool showBattleBackgroundOnAwake = true;

        private Coroutine transitionCoroutine;

        public bool IsTransitioning => transitionCoroutine != null;
        public bool IsStoreBackgroundVisible { get; private set; }

        private void Awake()
        {
            ValidateReferences();

            IsStoreBackgroundVisible = !showBattleBackgroundOnAwake;
            ApplyTargetAlpha();
        }

        public void ShowBattleBackground()
        {
            StartTransition(showStoreBackground: false);
        }

        public void ShowStoreBackground()
        {
            StartTransition(showStoreBackground: true);
        }

        private void StartTransition(bool showStoreBackground)
        {
            ValidateReferences();

            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
            }

            IsStoreBackgroundVisible = showStoreBackground;

            if (transitionDuration <= 0f)
            {
                transitionCoroutine = null;
                ApplyTargetAlpha();
                return;
            }

            transitionCoroutine = StartCoroutine(PlayTransition());
        }

        private IEnumerator PlayTransition()
        {
            float battleStartAlpha = battleBackgroundRenderer.color.a;
            float storeStartAlpha = storeBackgroundRenderer.color.a;
            float battleTargetAlpha = IsStoreBackgroundVisible ? 0f : 1f;
            float storeTargetAlpha = IsStoreBackgroundVisible ? 1f : 0f;
            float elapsed = 0f;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / transitionDuration);
                float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

                SetAlpha(
                    battleBackgroundRenderer,
                    Mathf.Lerp(battleStartAlpha, battleTargetAlpha, easedProgress));
                SetAlpha(
                    storeBackgroundRenderer,
                    Mathf.Lerp(storeStartAlpha, storeTargetAlpha, easedProgress));

                yield return null;
            }

            ApplyTargetAlpha();
            transitionCoroutine = null;
        }

        private void OnDisable()
        {
            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
                transitionCoroutine = null;
            }

            if (battleBackgroundRenderer != null && storeBackgroundRenderer != null)
            {
                ApplyTargetAlpha();
            }
        }

        private void ApplyTargetAlpha()
        {
            SetAlpha(battleBackgroundRenderer, IsStoreBackgroundVisible ? 0f : 1f);
            SetAlpha(storeBackgroundRenderer, IsStoreBackgroundVisible ? 1f : 0f);
        }

        private static void SetAlpha(SpriteRenderer spriteRenderer, float alpha)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }

        private void ValidateReferences()
        {
            if (battleBackgroundRenderer == null)
            {
                throw new InvalidOperationException(
                    "Battle background renderer is not assigned.");
            }

            if (storeBackgroundRenderer == null)
            {
                throw new InvalidOperationException(
                    "Store background renderer is not assigned.");
            }

            if (battleBackgroundRenderer == storeBackgroundRenderer)
            {
                throw new InvalidOperationException(
                    "Battle and store backgrounds must use different SpriteRenderers.");
            }
        }
    }
}
