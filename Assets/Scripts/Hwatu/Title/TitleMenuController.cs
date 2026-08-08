using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Hwatu.Title
{
    public sealed class TitleMenuController : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private RectTransform selectionFlame;
        [SerializeField] private string gameplaySceneName = "CombatExample";
        [SerializeField] private float flameGap = 5f;

        private GameObject lastSelection;

        private void OnEnable()
        {
            PrepareInput();
            startButton.onClick.AddListener(StartGame);
            quitButton.onClick.AddListener(QuitGame);
        }

        private void Start()
        {
            Select(startButton);
        }

        private void PrepareInput()
        {
            startButton.interactable = true;
            quitButton.interactable = true;
            SetButtonPanelTransparent(startButton);
            SetButtonPanelTransparent(quitButton);

            Canvas canvas = GetComponentInParent<Canvas>();
            CanvasScaler canvasScaler = canvas != null ? canvas.GetComponent<CanvasScaler>() : null;
            if (canvasScaler != null)
            {
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
                canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                canvasScaler.matchWidthOrHeight = 0.5f;
            }

            Transform backgroundTransform = canvas != null ? canvas.transform.Find("Background") : null;
            Image background = backgroundTransform != null
                ? backgroundTransform.GetComponent<Image>()
                : null;
            if (background != null)
            {
                background.raycastTarget = false;
            }
        }

        private static void SetButtonPanelTransparent(Button button)
        {
            Image panel = button.targetGraphic as Image;
            if (panel == null)
            {
                return;
            }

            Color color = panel.color;
            color.a = 0f;
            panel.color = color;
        }

        private void Update()
        {
            GameObject selection = EventSystem.current != null
                ? EventSystem.current.currentSelectedGameObject
                : null;

            lastSelection = selection;
            UpdateFlame(selection);
        }

        private void OnDisable()
        {
            startButton.onClick.RemoveListener(StartGame);
            quitButton.onClick.RemoveListener(QuitGame);
        }

        public void Select(Button button)
        {
            if (button == null || !button.IsInteractable())
            {
                return;
            }

            button.Select();
            lastSelection = button.gameObject;
            UpdateFlame(lastSelection);
        }

        private void UpdateFlame(GameObject selection)
        {
            RectTransform selectedRect = selection != null ? selection.GetComponent<RectTransform>() : null;
            if (selectedRect == null || (selection != startButton.gameObject && selection != quitButton.gameObject))
            {
                selectionFlame.gameObject.SetActive(false);
                return;
            }

            RectTransform parent = selectionFlame.parent as RectTransform;
            Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(parent, selectedRect);
            Text label = selection.GetComponentInChildren<Text>();
            float contentWidth = label != null ? label.preferredWidth : bounds.size.x;
            float contentLeft = bounds.center.x - contentWidth * 0.5f;
            selectionFlame.anchoredPosition = new Vector2(
                contentLeft - selectionFlame.rect.width * 0.5f - flameGap,
                bounds.center.y);
            selectionFlame.gameObject.SetActive(true);
        }

        private void StartGame()
        {
            SceneManager.LoadScene(gameplaySceneName);
        }

        private static void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
