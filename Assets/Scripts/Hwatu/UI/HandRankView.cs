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

        private GameObject previewBackdrop;

        private void Awake()
        {
            ValidateReferences();
            CreatePreviewBackdrop();
            previewPanel.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ValidateReferences();

            bool shouldOpen = !previewPanel.activeSelf;
            if (shouldOpen)
            {
                previewBackdrop.SetActive(true);
                previewBackdrop.transform.SetAsLastSibling();
                previewPanel.transform.SetAsLastSibling();
            }

            previewPanel.SetActive(shouldOpen);
            if (!shouldOpen)
            {
                previewBackdrop.SetActive(false);
            }
        }

        private void OnDisable()
        {
            ClosePreview();
        }

        private void ValidateReferences()
        {
            if (previewPanel == null)
            {
                throw new InvalidOperationException(
                    "Hand rank preview panel is not assigned.");
            }
        }

        private void CreatePreviewBackdrop()
        {
            GameObject backdropObject = new GameObject(
                "HandRankPreviewBackdrop",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button));
            backdropObject.transform.SetParent(
                previewPanel.transform.parent,
                worldPositionStays: false);

            RectTransform backdropRect = backdropObject.GetComponent<RectTransform>();
            backdropRect.anchorMin = Vector2.zero;
            backdropRect.anchorMax = Vector2.one;
            backdropRect.offsetMin = Vector2.zero;
            backdropRect.offsetMax = Vector2.zero;

            Image backdropImage = backdropObject.GetComponent<Image>();
            backdropImage.color = Color.clear;
            backdropImage.raycastTarget = true;

            Button backdropButton = backdropObject.GetComponent<Button>();
            backdropButton.transition = Selectable.Transition.None;
            backdropButton.targetGraphic = backdropImage;
            backdropButton.navigation = new Navigation
            {
                mode = Navigation.Mode.None
            };
            backdropButton.onClick.AddListener(ClosePreview);

            previewBackdrop = backdropObject;
            previewBackdrop.SetActive(false);
        }

        private void ClosePreview()
        {
            if (previewPanel != null)
            {
                previewPanel.SetActive(false);
            }

            if (previewBackdrop != null)
            {
                previewBackdrop.SetActive(false);
            }
        }
    }
}
