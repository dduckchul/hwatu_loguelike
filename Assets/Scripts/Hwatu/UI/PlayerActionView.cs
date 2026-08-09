using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hwatu.UI
{
    [DisallowMultipleComponent]
    public sealed class PlayerActionView : MonoBehaviour
    {
        private const string RerollAvailableText = "전투당 1회";
        private const string RerollUsedText = "사용 완료";

        [SerializeField] private Button submitButton;
        [SerializeField] private Button rerollButton;
        [SerializeField] private TMP_Text rerollUsageText;

        public event Action SubmitClicked;
        public event Action RerollClicked;

        private void Awake()
        {
            ValidateReferences();
            SetSubmitInteractable(false);
            SetRerollState(isInteractable: false, isUsed: false);
        }

        private void OnEnable()
        {
            if (submitButton != null)
            {
                submitButton.onClick.AddListener(HandleSubmitClicked);
            }

            if (rerollButton != null)
            {
                rerollButton.onClick.AddListener(HandleRerollClicked);
            }
        }

        private void OnDisable()
        {
            if (submitButton != null)
            {
                submitButton.onClick.RemoveListener(HandleSubmitClicked);
            }

            if (rerollButton != null)
            {
                rerollButton.onClick.RemoveListener(HandleRerollClicked);
            }
        }

        public void SetSubmitInteractable(bool isInteractable)
        {
            submitButton.interactable = isInteractable;
        }

        public void SetRerollState(bool isInteractable, bool isUsed)
        {
            rerollButton.interactable = isInteractable && !isUsed;
            rerollUsageText.text = isUsed
                ? RerollUsedText
                : RerollAvailableText;
        }

        private void HandleSubmitClicked()
        {
            SubmitClicked?.Invoke();
        }

        private void HandleRerollClicked()
        {
            RerollClicked?.Invoke();
        }

        private void ValidateReferences()
        {
            if (submitButton == null)
            {
                throw new InvalidOperationException("Submit button is not assigned.");
            }

            if (rerollButton == null)
            {
                throw new InvalidOperationException("Reroll button is not assigned.");
            }

            if (rerollUsageText == null)
            {
                throw new InvalidOperationException("Reroll usage text is not assigned.");
            }
        }
    }
}
