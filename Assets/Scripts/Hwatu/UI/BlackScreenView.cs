using System;
using System.Collections;
using UnityEngine;

namespace Hwatu.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class BlackScreenView : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float fadeDuration = 0.5f;

        private CanvasGroup canvasGroup;

        public bool IsTransitioning { get; private set; }

        private void Awake()
        {
            ValidateReferences();
            transform.SetAsLastSibling();
            SetAlpha(1f);
            SetInteractionEnabled(true);
        }

        private IEnumerator Start()
        {
            yield return FadeFromBlack();
        }

        public IEnumerator FadeFromBlack()
        {
            ValidateTransitionState();
            IsTransitioning = true;
            gameObject.SetActive(true);
            SetInteractionEnabled(true);

            yield return Fade(1f, 0f);

            SetInteractionEnabled(false);
            IsTransitioning = false;
            gameObject.SetActive(false);
        }

        public IEnumerator FadeToBlack()
        {
            ValidateTransitionState();
            IsTransitioning = true;
            gameObject.SetActive(true);
            SetInteractionEnabled(true);

            yield return Fade(0f, 1f);

            IsTransitioning = false;
        }

        private IEnumerator Fade(float from, float to)
        {
            SetAlpha(from);
            yield return null;

            if (fadeDuration > 0f)
            {
                double fadeStartedAt = Time.realtimeSinceStartupAsDouble;
                while (true)
                {
                    double elapsed = Time.realtimeSinceStartupAsDouble - fadeStartedAt;
                    float progress = Mathf.Clamp01((float)(elapsed / fadeDuration));
                    SetAlpha(Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, progress)));

                    if (progress >= 1f)
                    {
                        break;
                    }

                    yield return null;
                }
            }

            SetAlpha(to);
        }

        private void ValidateReferences()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                throw new InvalidOperationException(
                    "Black screen requires a CanvasGroup.");
            }
        }

        private void ValidateTransitionState()
        {
            ValidateReferences();
            if (IsTransitioning)
            {
                throw new InvalidOperationException(
                    "Black screen is already transitioning.");
            }
        }

        private void SetAlpha(float alpha)
        {
            canvasGroup.alpha = Mathf.Clamp01(alpha);
        }

        private void SetInteractionEnabled(bool isEnabled)
        {
            canvasGroup.interactable = isEnabled;
            canvasGroup.blocksRaycasts = isEnabled;
        }
    }
}
