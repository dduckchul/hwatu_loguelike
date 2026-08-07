using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hwatu.UI
{
    [DisallowMultipleComponent]
    public sealed class HandRankPreviewHover : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        [SerializeField] private GameObject previewPanel;

        private void Awake()
        {
            ValidateReferences();
            DisablePreviewRaycasts();
            previewPanel.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ValidateReferences();
            previewPanel.transform.SetAsLastSibling();
            previewPanel.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (previewPanel != null)
            {
                previewPanel.SetActive(false);
            }
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
