using System;
using Microsoft.CodeAnalysis.Operations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hwatu.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button), typeof(Image))]
    public sealed class HoverButtonView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private Color normalButtonColor = new Color32(40, 40, 65, 255);
        [SerializeField] private Color hoveredButtonColor = Color.softRed;
        [SerializeField] private Color normalTextColor = Color.white;
        [SerializeField] private Color hoveredTextColor = Color.white;

        private Button targetButton;
        private Image hoverImage;

        private void Awake()
        {
            targetButton = GetComponent<Button>();
            hoverImage = GetComponent<Image>();
            ValidateReferences();
            SetHovered(false);
        }

        private void OnEnable()
        {
            if (targetButton != null && hoverImage != null && labelText != null)
            {
                SetHovered(false);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (targetButton == null || !targetButton.interactable)
            {
                return;
            }

            SetHovered(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetHovered(false);
        }

        public void SetInteractionEnabled(bool isEnabled)
        {
            targetButton.interactable = isEnabled;

            if (!isEnabled)
            {
                SetHovered(false);
            }
        }

        private void SetHovered(bool isHovered)
        {
            hoverImage.color = isHovered ? hoveredButtonColor : normalButtonColor;
            labelText.color = isHovered ? hoveredTextColor : normalTextColor;
        }

        private void ValidateReferences()
        {
            if (targetButton == null || hoverImage == null)
            {
                throw new InvalidOperationException(
                    "Reward button requires Button and Image components.");
            }

            if (labelText == null)
            {
                throw new InvalidOperationException("Reward button label text is not assigned.");
            }
        }
    }
}
