using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hwatu.UI
{
    [DisallowMultipleComponent]
    public sealed class RunCompleteView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Transform uiRoot;
        [SerializeField] private GameObject completionRoot;
        [SerializeField] private TMP_Text completionText;

        [Header("Transition")]
        [SerializeField, Min(0f)] private float titleLoadDelay = 1f;
        [SerializeField] private string titleSceneName = "TitleScene";

        private Coroutine completionSequence;

        public bool IsPlaying => completionSequence != null;

        private void Awake()
        {
            ValidateReferences();
            completionRoot.SetActive(false);
        }

        public void Show()
        {
            if (IsPlaying)
            {
                return;
            }

            ValidateReferences();
            completionSequence = StartCoroutine(ShowCore());
        }

        private IEnumerator ShowCore()
        {
            HideExistingUi();
            completionText.text = "To Be Continued..";
            completionRoot.SetActive(true);

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
                if (child != completionRoot)
                {
                    child.SetActive(false);
                }
            }
        }

        private void OnDisable()
        {
            if (completionSequence == null)
            {
                return;
            }

            StopCoroutine(completionSequence);
            completionSequence = null;
        }

        private void ValidateReferences()
        {
            if (uiRoot == null)
            {
                throw new InvalidOperationException("UI root is not assigned.");
            }

            if (completionRoot == null)
            {
                throw new InvalidOperationException("Completion root is not assigned.");
            }

            if (completionRoot.transform.parent != uiRoot)
            {
                throw new InvalidOperationException(
                    "Completion root must be a direct child of the UI root.");
            }

            if (completionText == null)
            {
                throw new InvalidOperationException("Completion text is not assigned.");
            }

            if (string.IsNullOrWhiteSpace(titleSceneName))
            {
                throw new InvalidOperationException("Title scene name is empty.");
            }
        }
    }
}
