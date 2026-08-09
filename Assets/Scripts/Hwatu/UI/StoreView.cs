using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hwatu.UI
{
    [DisallowMultipleComponent]
    public sealed class StoreView : MonoBehaviour
    {
        private const int DetailFontSizePercent = 50;

        [SerializeField] private Button upgradeButton;
        [SerializeField] private TMP_Text upgradeButtonText;
        [SerializeField] private Button removalButton;
        [SerializeField] private TMP_Text removalButtonText;

        public event Action UpgradeRequested;
        public event Action RemovalRequested;
        public event Action SkipRequested;

        public bool IsOpen => gameObject.activeSelf;

        private void OnEnable()
        {
            if (upgradeButton != null)
            {
                upgradeButton.onClick.AddListener(RequestUpgrade);
            }

            if (removalButton != null)
            {
                removalButton.onClick.AddListener(RequestRemoval);
            }
        }

        private void OnDisable()
        {
            if (upgradeButton != null)
            {
                upgradeButton.onClick.RemoveListener(RequestUpgrade);
            }

            if (removalButton != null)
            {
                removalButton.onClick.RemoveListener(RequestRemoval);
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetUpgradeState(int cost, bool interactable, bool used)
        {
            if (cost < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cost));
            }

            ValidateUpgradeReferences();
            upgradeButton.interactable = interactable;
            upgradeButtonText.text = used
                ? CreateTwoLineLabel("강화", "사용 완료")
                : CreateTwoLineLabel("강화", $"{cost}전");
        }

        public void SetRemovalState(int cost, bool interactable, bool used)
        {
            if (cost < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cost));
            }

            ValidateRemovalReferences();
            removalButton.interactable = interactable;
            removalButtonText.text = used
                ? CreateTwoLineLabel("버리기", "사용 완료")
                : CreateTwoLineLabel("버리기", $"{cost}전");
        }

        public void RequestUpgrade()
        {
            UpgradeRequested?.Invoke();
        }

        public void RequestRemoval()
        {
            RemovalRequested?.Invoke();
        }

        public void RequestSkip()
        {
            SkipRequested?.Invoke();
        }

    private static string CreateTwoLineLabel(string title, string detail)
    {
        return $"{title}\n<size={DetailFontSizePercent}%>({detail})</size>";
    }

        private void Reset()
        {
            upgradeButton = transform.Find("EnforcementButton")
                ?.GetComponent<Button>();
            upgradeButtonText = transform.Find("EnforcementButton/Text (TMP)")
                ?.GetComponent<TMP_Text>();
            removalButton = transform.Find("DiscardButton")
                ?.GetComponent<Button>();
            removalButtonText = transform.Find("DiscardButton/Text (TMP)")
                ?.GetComponent<TMP_Text>();
        }

        private void ValidateUpgradeReferences()
        {
            if (upgradeButton == null)
            {
                throw new InvalidOperationException(
                    "Upgrade button is not assigned.");
            }

            if (upgradeButtonText == null)
            {
                throw new InvalidOperationException(
                    "Upgrade button text is not assigned.");
            }
        }

        private void ValidateRemovalReferences()
        {
            if (removalButton == null)
            {
                throw new InvalidOperationException(
                    "Removal button is not assigned.");
            }

            if (removalButtonText == null)
            {
                throw new InvalidOperationException(
                    "Removal button text is not assigned.");
            }
        }
    }
}
