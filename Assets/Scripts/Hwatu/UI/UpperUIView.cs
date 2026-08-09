using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hwatu.UI
{
    [DisallowMultipleComponent]
    public sealed class UpperUIView : MonoBehaviour
    {
        [SerializeField] private Image moneyIcon;
        [SerializeField] private TMP_Text moneyText;
        [SerializeField] private TMP_Text moneyDeltaText;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text subText;
        [SerializeField] private Image handRankIcon;

        [Header("Money Delta")]
        [SerializeField, Min(0f)] private float moneyDeltaDisplayDuration = 1f;
        [SerializeField] private Color moneyGainColor = new Color32(47, 128, 237, 255);
        [SerializeField] private Color moneyLossColor = new Color32(235, 87, 87, 255);

        private Coroutine hideMoneyDeltaCoroutine;
        private int displayedMoney;
        private bool hasDisplayedMoney;

        private void Awake()
        {
            ValidateReferences();
            moneyDeltaText.gameObject.SetActive(false);
        }

        public void ShowBattle(int battleNumber, int stake)
        {
            if (battleNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(battleNumber),
                    battleNumber,
                    "Battle number must be greater than zero.");
            }

            if (stake < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(stake),
                    stake,
                    "Stake cannot be negative.");
            }

            ValidateReferences();
            titleText.text = $"제 {battleNumber} 회전";
            subText.text = $"판 돈 | {stake} 전";
        }

        public void ShowStore()
        {
            ValidateReferences();
            titleText.text = "귀 시 장";
            subText.text = "천천히 둘러보세요";
        }

        public void ShowMoney(int money)
        {
            if (money < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(money),
                    money,
                    "Displayed money cannot be negative.");
            }

            ValidateReferences();
            moneyText.text = $"{money} 전";

            if (!hasDisplayedMoney)
            {
                displayedMoney = money;
                hasDisplayedMoney = true;
                return;
            }

            int delta = money - displayedMoney;
            displayedMoney = money;
            if (delta != 0)
            {
                ShowMoneyDelta(delta);
            }
        }

        private void ShowMoneyDelta(int delta)
        {
            if (hideMoneyDeltaCoroutine != null)
            {
                StopCoroutine(hideMoneyDeltaCoroutine);
            }

            moneyDeltaText.color = delta > 0 ? moneyGainColor : moneyLossColor;
            moneyDeltaText.text = delta > 0
                ? $"+ {delta} 전"
                : $"- {Math.Abs(delta)} 전";
            moneyDeltaText.gameObject.SetActive(true);

            hideMoneyDeltaCoroutine = StartCoroutine(HideMoneyDeltaAfterDelay());
        }

        private IEnumerator HideMoneyDeltaAfterDelay()
        {
            if (moneyDeltaDisplayDuration > 0f)
            {
                yield return new WaitForSecondsRealtime(moneyDeltaDisplayDuration);
            }

            moneyDeltaText.gameObject.SetActive(false);
            hideMoneyDeltaCoroutine = null;
        }

        private void OnDisable()
        {
            if (hideMoneyDeltaCoroutine != null)
            {
                StopCoroutine(hideMoneyDeltaCoroutine);
                hideMoneyDeltaCoroutine = null;
            }

            if (moneyDeltaText != null)
            {
                moneyDeltaText.gameObject.SetActive(false);
            }
        }

        private void ValidateReferences()
        {
            if (titleText == null)
            {
                throw new InvalidOperationException("Upper UI title text is not assigned.");
            }

            if (moneyText == null)
            {
                throw new InvalidOperationException("Upper UI money text is not assigned.");
            }

            if (moneyDeltaText == null)
            {
                throw new InvalidOperationException(
                    "Upper UI money delta text is not assigned.");
            }

            if (subText == null)
            {
                throw new InvalidOperationException("Upper UI subtext is not assigned.");
            }
        }
    }
}
