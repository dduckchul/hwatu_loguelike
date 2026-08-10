using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Hwatu.Intro
{
    public sealed class IntroSequenceController : MonoBehaviour
    {
        public const string FirstText =
            "「집사(執事) 비형은 들으라.\n비광과 그 무리가 귀시장으로 달아났으니—」";

        public const string SecondText =
            "「단 하나의 혼도 놓치지 말라\n이는 어명이니, 속히 집행하라.」";

        [Header("Pages")]
        [SerializeField] private Image background;
        [SerializeField] private Sprite intro1;
        [SerializeField] private Sprite intro2;
        [SerializeField] private Text storyText1;
        [SerializeField] private Text storyText2;
        [SerializeField] private Text continueHint;
        [SerializeField] private Image fadeOverlay;

        [Header("Timing")]
        [SerializeField, Min(0.01f)] private float imageFadeDuration = 0.6f;
        [SerializeField, Min(0.01f)] private float textFadeDuration = 0.4f;
        [SerializeField, Min(0f)] private float lineDelay = 1f;
        [SerializeField, Min(0f)] private float secondPageFirstLineDelay = 1f;
        [SerializeField, Min(0.01f)] private float transitionFadeDuration = 0.6f;

        private string firstPageText;
        private string secondPageText;
        private Color firstTextColor;
        private Color secondTextColor;
        private Color backgroundColor;
        private int pageIndex;
        private bool awaitingAdvance;
        private bool isTransitioning;

        private void Awake()
        {
            firstPageText = storyText1.text;
            secondPageText = storyText2.text;
            firstTextColor = storyText1.color;
            secondTextColor = storyText2.color;
            backgroundColor = background.color;
        }

        private void Start()
        {
            pageIndex = 0;
            StartCoroutine(PlayPage());
        }

        private void Update()
        {
            if (!awaitingAdvance || isTransitioning || !WasAdvancePressed())
            {
                return;
            }

            awaitingAdvance = false;
            StartCoroutine(pageIndex == 0 ? TransitionToSecondPage() : TransitionToCombat());
        }

        private IEnumerator PlayPage()
        {
            Text activeText = GetActiveText();
            Text inactiveText = pageIndex == 0 ? storyText2 : storyText1;
            string content = pageIndex == 0 ? firstPageText : secondPageText;
            Color contentColor = pageIndex == 0 ? firstTextColor : secondTextColor;

            background.sprite = pageIndex == 0 ? intro1 : intro2;
            activeText.gameObject.SetActive(true);
            inactiveText.gameObject.SetActive(false);
            continueHint.gameObject.SetActive(false);
            SetAllLinesAlpha(activeText, content, contentColor, 0f);

            SetGraphicAlpha(background, backgroundColor.a);
            yield return FadeOverlay(1f, 0f, imageFadeDuration);

            if (pageIndex == 1 && secondPageFirstLineDelay > 0f)
            {
                yield return new WaitForSecondsRealtime(secondPageFirstLineDelay);
            }

            yield return RevealLines(activeText, content, contentColor);
            continueHint.text = pageIndex == 0 ? "계속" : "시작";
            continueHint.gameObject.SetActive(true);
            awaitingAdvance = true;
        }

        private IEnumerator RevealLines(Text target, string content, Color color)
        {
            string[] lines = SplitLines(content);
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                float elapsed = 0f;
                while (elapsed < textFadeDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float alpha = Mathf.Clamp01(elapsed / textFadeDuration);
                    SetLineAlphas(target, lines, color, lineIndex, alpha);
                    yield return null;
                }

                SetLineAlphas(target, lines, color, lineIndex, 1f);
                if (lineIndex < lines.Length - 1 && lineDelay > 0f)
                {
                    yield return new WaitForSecondsRealtime(lineDelay);
                }
            }
        }

        private IEnumerator TransitionToSecondPage()
        {
            isTransitioning = true;
            continueHint.gameObject.SetActive(false);
            yield return FadeOverlay(0f, 1f, transitionFadeDuration);
            pageIndex = 1;
            yield return PlayPage();
            isTransitioning = false;
        }

        private IEnumerator TransitionToCombat()
        {
            isTransitioning = true;
            continueHint.gameObject.SetActive(false);
            yield return FadeOverlay(0f, 1f, transitionFadeDuration);
            SceneManager.LoadScene("CombatExample");
        }

        private IEnumerator FadeOverlay(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
                SetGraphicAlpha(fadeOverlay, alpha);
                yield return null;
            }

            SetGraphicAlpha(fadeOverlay, to);
        }

        private Text GetActiveText()
        {
            return pageIndex == 0 ? storyText1 : storyText2;
        }

        private static string[] SplitLines(string content)
        {
            return content.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        }

        private static void SetAllLinesAlpha(Text target, string content, Color color, float alpha)
        {
            string[] lines = SplitLines(content);
            SetLineAlphas(target, lines, color, -1, alpha);
        }

        private static void SetLineAlphas(
            Text target,
            string[] lines,
            Color color,
            int activeLineIndex,
            float activeLineAlpha)
        {
            StringBuilder builder = new StringBuilder();
            string rgb = ColorUtility.ToHtmlStringRGB(color);
            for (int index = 0; index < lines.Length; index++)
            {
                float alpha = index < activeLineIndex
                    ? 1f
                    : index == activeLineIndex
                        ? activeLineAlpha
                        : 0f;
                int alphaByte = Mathf.RoundToInt(alpha * color.a * 255f);
                builder.Append("<color=#")
                    .Append(rgb)
                    .Append(alphaByte.ToString("X2"))
                    .Append('>')
                    .Append(lines[index])
                    .Append("</color>");
                if (index < lines.Length - 1)
                {
                    builder.Append('\n');
                }
            }

            target.color = Color.white;
            target.text = builder.ToString();
        }

        private static void SetGraphicAlpha(Graphic graphic, float alpha)
        {
            Color color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }

        private static bool WasAdvancePressed()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null &&
                (keyboard.enterKey.wasPressedThisFrame ||
                 keyboard.numpadEnterKey.wasPressedThisFrame ||
                 keyboard.spaceKey.wasPressedThisFrame))
            {
                return true;
            }

            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            {
                return true;
            }

            Touchscreen touchscreen = Touchscreen.current;
            return touchscreen != null
                && touchscreen.primaryTouch.press.wasPressedThisFrame;
        }
    }
}
