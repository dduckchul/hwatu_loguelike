using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hwatu.UI
{
    [DisallowMultipleComponent]
    public sealed class StoreView : MonoBehaviour
    {
        [SerializeField] private Button upgradeButton;
        [SerializeField] private TMP_Text upgradeButtonText;

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
        }

        private void OnDisable()
        {
            if (upgradeButton != null)
            {
                upgradeButton.onClick.RemoveListener(RequestUpgrade);
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
                ? "강화\n(사용 완료)"
                : $"강화\n({cost}전)";
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

        private void Reset()
        {
            upgradeButton = transform.Find("EnforcementButton")
                ?.GetComponent<Button>();
            upgradeButtonText = transform.Find("EnforcementButton/Text (TMP)")
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
    }
}
