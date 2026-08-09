using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hwatu.UI
{
    [DisallowMultipleComponent]
    public sealed class HandRankPreviewHover : MonoBehaviour,
        IPointerClickHandler
    {
        [SerializeField] private GameObject previewPanel;

        private void Awake()
        {
            ValidateReferences();
            DisablePreviewRaycasts();
            previewPanel.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ValidateReferences();

            bool shouldOpen = !previewPanel.activeSelf;
            if (shouldOpen)
            {
                previewPanel.transform.SetAsLastSibling();
            }

            previewPanel.SetActive(shouldOpen);
        }

        private void OnDisable()
        {
            if (previewPanel != null)
            {
                previewPanel.SetActive(false);
            }
        }

        private void ValidateReferences()
        {
            if (previewPanel == null)
            {
                throw new InvalidOperationException(
                    "Hand rank preview panel is not assigned.");
            }
        }

        private void DisablePreviewRaycasts()
        {
            Graphic[] graphics = previewPanel.GetComponentsInChildren<Graphic>(
                includeInactive: true);
            foreach (Graphic graphic in graphics)
            {
                graphic.raycastTarget = false;
            }
        }
    }
}
