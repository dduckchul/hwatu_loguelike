using System;
using System.Collections;
using System.Collections.Generic;
using Hwatu.UI;
using TMPro;
using UnityEngine;

namespace Hwatu.Rewards
{
    [DisallowMultipleComponent]
    public sealed class StoreController : MonoBehaviour
    {
        [Header("Transition Views")]
        [SerializeField] private BackgroundTransitionView backgroundTransitionView;
        [SerializeField] private CanvasGroup battleUiCanvasGroup;
        [SerializeField] private Transform playerRoot;
        [SerializeField] private List<GameObject> enemyRoots = new List<GameObject>();

        [Header("Store")]
        [SerializeField] private StoreView storeView;
        [SerializeField] private CardStoreController cardStoreController;

        [Header("Fade")]
        [SerializeField, Min(0f)] private float presentationFadeDuration = 0.5f;

        [Header("Player Position")]
        [SerializeField] private Vector3 storePlayerLocalPosition =
            new Vector3(0f, 0.5f, 0f);

        private readonly List<SpriteFadeTarget> enemySpriteTargets =
            new List<SpriteFadeTarget>();
        private readonly List<TextFadeTarget> enemyTextTargets =
            new List<TextFadeTarget>();
        private Coroutine transitionCoroutine;
        private float battleUiVisibleAlpha;
        private Vector3 battlePlayerLocalPosition;
        private bool targetStoreOpen;

        public event Action StoreOpened;
        public event Action BattlePresentationRestored;

        public bool IsStoreOpen { get; private set; }
        public bool IsTransitioning => transitionCoroutine != null;

        private sealed class SpriteFadeTarget
        {
            public SpriteRenderer Renderer { get; }
            public Color VisibleColor { get; }

            public SpriteFadeTarget(SpriteRenderer renderer)
            {
                Renderer = renderer;
                VisibleColor = renderer.color;
            }
        }

        private sealed class TextFadeTarget
        {
            public TMP_Text Text { get; }
            public Color VisibleColor { get; }

            public TextFadeTarget(TMP_Text text)
            {
                Text = text;
                VisibleColor = text.color;
            }
        }

        private void Awake()
        {
            ValidateReferences();
            battleUiVisibleAlpha = battleUiCanvasGroup.alpha;
            battlePlayerLocalPosition = playerRoot.localPosition;
            SetBattleUiInteractionEnabled(true);
            cardStoreController.Close();
            storeView.Hide();
        }

        private void OnEnable()
        {
            if (storeView != null)
            {
                storeView.SkipRequested += ExitStore;
            }
        }

        public void EnterStore()
        {
            if (IsTransitioning || IsStoreOpen)
            {
                return;
            }

            ValidateReferences();
            CaptureEnemyVisuals();
            targetStoreOpen = true;
            transitionCoroutine = StartCoroutine(EnterStoreCore());
        }

        public void ExitStore()
        {
            if (IsTransitioning || !IsStoreOpen)
            {
                return;
            }

            ValidateReferences();
            cardStoreController.Close();
            storeView.Hide();
            targetStoreOpen = false;
            transitionCoroutine = StartCoroutine(ExitStoreCore());
        }

        private IEnumerator EnterStoreCore()
        {
            SetBattleUiInteractionEnabled(false);
            backgroundTransitionView.ShowStoreBackground();

            yield return FadePresentation(showBattlePresentation: false);
            yield return WaitForBackgroundTransition();

            IsStoreOpen = true;
            transitionCoroutine = null;
            storeView.Show();
            cardStoreController.Open();
            StoreOpened?.Invoke();
        }

        private IEnumerator ExitStoreCore()
        {
            backgroundTransitionView.ShowBattleBackground();

            yield return FadePresentation(showBattlePresentation: true);
            yield return WaitForBackgroundTransition();

            IsStoreOpen = false;
            transitionCoroutine = null;
            SetBattleUiInteractionEnabled(true);
            BattlePresentationRestored?.Invoke();
        }

        private IEnumerator FadePresentation(bool showBattlePresentation)
        {
            float startBattleUiAlpha = battleUiCanvasGroup.alpha;
            float targetBattleUiAlpha = showBattlePresentation
                ? battleUiVisibleAlpha
                : 0f;
            float startVisibility = GetEnemyVisibility();
            float targetVisibility = showBattlePresentation ? 1f : 0f;
            Vector3 startPlayerPosition = playerRoot.localPosition;
            Vector3 targetPlayerPosition = showBattlePresentation
                ? battlePlayerLocalPosition
                : storePlayerLocalPosition;

            if (presentationFadeDuration <= 0f)
            {
                battleUiCanvasGroup.alpha = targetBattleUiAlpha;
                SetEnemyVisibility(targetVisibility);
                playerRoot.localPosition = targetPlayerPosition;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < presentationFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / presentationFadeDuration);
                float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

                battleUiCanvasGroup.alpha = Mathf.Lerp(
                    startBattleUiAlpha,
                    targetBattleUiAlpha,
                    easedProgress);
                SetEnemyVisibility(
                    Mathf.Lerp(startVisibility, targetVisibility, easedProgress));
                playerRoot.localPosition = Vector3.Lerp(
                    startPlayerPosition,
                    targetPlayerPosition,
                    easedProgress);

                yield return null;
            }

            battleUiCanvasGroup.alpha = targetBattleUiAlpha;
            SetEnemyVisibility(targetVisibility);
            playerRoot.localPosition = targetPlayerPosition;
        }

        private IEnumerator WaitForBackgroundTransition()
        {
            while (backgroundTransitionView.IsTransitioning)
            {
                yield return null;
            }
        }

        private void CaptureEnemyVisuals()
        {
            enemySpriteTargets.Clear();
            enemyTextTargets.Clear();

            foreach (GameObject enemyRoot in enemyRoots)
            {
                SpriteRenderer[] spriteRenderers =
                    enemyRoot.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
                foreach (SpriteRenderer spriteRenderer in spriteRenderers)
                {
                    enemySpriteTargets.Add(new SpriteFadeTarget(spriteRenderer));
                }

                TMP_Text[] texts = enemyRoot.GetComponentsInChildren<TMP_Text>(includeInactive: true);
                foreach (TMP_Text text in texts)
                {
                    enemyTextTargets.Add(new TextFadeTarget(text));
                }
            }
        }

        private float GetEnemyVisibility()
        {
            foreach (SpriteFadeTarget target in enemySpriteTargets)
            {
                if (target.Renderer != null && target.VisibleColor.a > 0f)
                {
                    return Mathf.Clamp01(target.Renderer.color.a / target.VisibleColor.a);
                }
            }

            foreach (TextFadeTarget target in enemyTextTargets)
            {
                if (target.Text != null && target.VisibleColor.a > 0f)
                {
                    return Mathf.Clamp01(target.Text.color.a / target.VisibleColor.a);
                }
            }

            return targetStoreOpen ? 0f : 1f;
        }

        private void SetEnemyVisibility(float visibility)
        {
            visibility = Mathf.Clamp01(visibility);

            foreach (SpriteFadeTarget target in enemySpriteTargets)
            {
                if (target.Renderer == null)
                {
                    continue;
                }

                Color color = target.VisibleColor;
                color.a *= visibility;
                target.Renderer.color = color;
            }

            foreach (TextFadeTarget target in enemyTextTargets)
            {
                if (target.Text == null)
                {
                    continue;
                }

                Color color = target.VisibleColor;
                color.a *= visibility;
                target.Text.color = color;
            }
        }

        private void SetBattleUiInteractionEnabled(bool isEnabled)
        {
            battleUiCanvasGroup.interactable = isEnabled;
            battleUiCanvasGroup.blocksRaycasts = isEnabled;
        }

        private void OnDisable()
        {
            if (storeView != null)
            {
                storeView.SkipRequested -= ExitStore;
                storeView.Hide();
            }

            if (cardStoreController != null)
            {
                cardStoreController.Close();
            }

            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
                transitionCoroutine = null;
            }

            if (battleUiCanvasGroup == null)
            {
                return;
            }

            bool showBattlePresentation = !targetStoreOpen;
            battleUiCanvasGroup.alpha = showBattlePresentation
                ? battleUiVisibleAlpha
                : 0f;
            SetEnemyVisibility(showBattlePresentation ? 1f : 0f);
            if (playerRoot != null)
            {
                playerRoot.localPosition = showBattlePresentation
                    ? battlePlayerLocalPosition
                    : storePlayerLocalPosition;
            }

            SetBattleUiInteractionEnabled(showBattlePresentation);
            IsStoreOpen = targetStoreOpen;
        }

        private void ValidateReferences()
        {
            if (backgroundTransitionView == null)
            {
                throw new InvalidOperationException(
                    "Background transition view is not assigned.");
            }

            if (battleUiCanvasGroup == null)
            {
                throw new InvalidOperationException(
                    "Battle UI canvas group is not assigned.");
            }

            if (playerRoot == null)
            {
                throw new InvalidOperationException(
                    "Player root is not assigned.");
            }

            if (storeView == null)
            {
                throw new InvalidOperationException(
                    "Store view is not assigned.");
            }

            if (cardStoreController == null)
            {
                throw new InvalidOperationException(
                    "Card store controller is not assigned.");
            }

            if (enemyRoots == null || enemyRoots.Count == 0)
            {
                throw new InvalidOperationException(
                    "At least one enemy root must be assigned.");
            }

            var uniqueEnemyRoots = new HashSet<GameObject>();
            for (int index = 0; index < enemyRoots.Count; index++)
            {
                GameObject enemyRoot = enemyRoots[index];
                if (enemyRoot == null)
                {
                    throw new InvalidOperationException(
                        $"Enemy root at index {index} is not assigned.");
                }

                if (!uniqueEnemyRoots.Add(enemyRoot))
                {
                    throw new InvalidOperationException(
                        "The same enemy root cannot be assigned more than once.");
                }
            }
        }
    }
}
