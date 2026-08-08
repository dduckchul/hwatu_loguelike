using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hwatu.Title
{
    public sealed class TitleMenuButton : UIBehaviour, IPointerEnterHandler
    {
        private Button button;

        protected override void Awake()
        {
            base.Awake();
            button = GetComponent<Button>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (button != null && button.IsInteractable())
            {
                button.Select();
            }
        }
    }
}
