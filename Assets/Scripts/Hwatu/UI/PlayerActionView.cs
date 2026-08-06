using System;
using UnityEngine;
using UnityEngine.UI;

namespace Hwatu.UI
{
    [DisallowMultipleComponent]
    public sealed class PlayerActionView : MonoBehaviour
    {
        [SerializeField] private Button submitButton;
        [SerializeField] private Button rerollButton;

        public event Action SubmitClicked;
        public event Action RerollClicked;

        private void Awake()
        {
            ValidateReferences();
            SetSubmitInteractable(false);
            SetRerollInteractable(false);
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

        public void SetRerollInteractable(bool isInteractable)
        {
            rerollButton.interactable = isInteractable;
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
        }
    }
}
