using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hwatu.UI
{
    [DisallowMultipleComponent]
    public sealed class DefeatView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Transform uiRoot;
        [SerializeField] private GameObject defeatOverlayRoot;
        [SerializeField] private CanvasGroup screenFadeCanvasGroup;
        [SerializeField] private TMP_Text defeatText;

        [Header("Transition")]
        [SerializeField, Min(0f)] private float fadeDuration = 0.5f;
        [SerializeField, Min(0f)] private float titleLoadDelay = 1f;
        [SerializeField] private string titleSceneName = "TitleScene";

        private Coroutine defeatSequence;

        public bool IsPlaying => defeatSequence != null;

        private void Awake()
        {
            ValidateReferences();
            screenFadeCanvasGroup.alpha = 0f;
            screenFadeCanvasGroup.interactable = false;
            screenFadeCanvasGroup.blocksRaycasts = false;
            defeatOverlayRoot.SetActive(false);
        }

        public void Play()
        {
            if (IsPlaying)
            {
                return;
            }

            ValidateReferences();
            defeatSequence = StartCoroutine(PlayCore());
        }

        private IEnumerator PlayCore()
        {
            HideExistingUi();

            defeatText.text = "사망";
            screenFadeCanvasGroup.alpha = 0f;
            screenFadeCanvasGroup.interactable = true;
            screenFadeCanvasGroup.blocksRaycasts = true;
            defeatOverlayRoot.SetActive(true);

            if (fadeDuration > 0f)
            {
                float elapsed = 0f;
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float progress = Mathf.Clamp01(elapsed / fadeDuration);
                    screenFadeCanvasGroup.alpha = Mathf.SmoothStep(0f, 1f, progress);
                    yield return null;
                }
            }

            screenFadeCanvasGroup.alpha = 1f;

            if (titleLoadDelay > 0f)
            {
                yield return new WaitForSecondsRealtime(titleLoadDelay);
            }

            SceneManager.LoadScene(titleSceneName);
        }

        private void HideExistingUi()
        {
            for (int index = 0; index < uiRoot.childCount; index++)
            {
                GameObject child = uiRoot.GetChild(index).gameObject;
                if (child != defeatOverlayRoot)
                {
                    child.SetActive(false);
                }
            }
        }

        private void OnDisable()
        {
            if (defeatSequence == null)
            {
                return;
            }

            StopCoroutine(defeatSequence);
            defeatSequence = null;
        }

        private void ValidateReferences()
        {
            if (uiRoot == null)
            {
                throw new InvalidOperationException("UI root is not assigned.");
            }

            if (defeatOverlayRoot == null)
            {
                throw new InvalidOperationException("Defeat overlay root is not assigned.");
            }

            if (defeatOverlayRoot.transform.parent != uiRoot)
            {
                throw new InvalidOperationException(
                    "Defeat overlay root must be a direct child of the UI root.");
            }

            if (screenFadeCanvasGroup == null)
            {
                throw new InvalidOperationException(
                    "Screen fade canvas group is not assigned.");
            }

            if (defeatText == null)
            {
                throw new InvalidOperationException("Defeat text is not assigned.");
            }

            if (string.IsNullOrWhiteSpace(titleSceneName))
            {
                throw new InvalidOperationException("Title scene name is empty.");
            }
        }
    }
}
