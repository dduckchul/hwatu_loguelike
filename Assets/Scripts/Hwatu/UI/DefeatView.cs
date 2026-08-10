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
        [SerializeField] private BlackScreenView blackScreenView;
        [SerializeField] private TMP_Text defeatText;

        [Header("Transition")]
        [SerializeField, Min(0f)] private float titleLoadDelay = 1f;
        [SerializeField] private string titleSceneName = "TitleScene";

        private Coroutine defeatSequence;

        public bool IsPlaying => defeatSequence != null;

        private void Awake()
        {
            ValidateReferences();
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
            defeatOverlayRoot.SetActive(true);
            blackScreenView.gameObject.SetActive(true);
            blackScreenView.transform.SetAsLastSibling();
            defeatOverlayRoot.transform.SetAsLastSibling();

            yield return blackScreenView.FadeToBlack();

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
                if (child != defeatOverlayRoot
                    && child != blackScreenView.gameObject)
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

            if (blackScreenView == null)
            {
                throw new InvalidOperationException(
                    "Black screen view is not assigned.");
            }

            if (blackScreenView.transform.parent != uiRoot)
            {
                throw new InvalidOperationException(
                    "Black screen view must be a direct child of the UI root.");
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
