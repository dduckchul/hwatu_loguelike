using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hwatu.UI
{
    public class RewardButtonView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Button _targetButton;
        private Image _image;
        private TMP_Text _button_text;
        private bool _isInteractionEnabled;
        
        private Color32 colorBlueBlack = new (40, 40, 65, 255);
        private Color colorWhite = Color.white;

        void Awake()
        {
            _targetButton = GetComponent<Button>();
            _image = GetComponent<Image>();
            _button_text = transform.GetChild(0).GetComponent<TMP_Text>();
        }

        void Start()
        {
            SetInteractionEnabled(_targetButton.IsActive());
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_targetButton == null || !_isInteractionEnabled)
            {
                return;
            }
            
            SetHovered(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_targetButton == null || !_isInteractionEnabled)
            {
                return;
            }
            
            SetHovered(false);
        }
        
        public void SetInteractionEnabled(bool isEnabled)
        {
            _isInteractionEnabled = isEnabled;
            
            if (!isEnabled)
            {
                SetHovered(false);
            }
        }
        public void SetHovered(bool isHovered)
        {
            _image.enabled = isHovered;
            _button_text.color = isHovered ? colorWhite : colorBlueBlack;
        }
    }
}