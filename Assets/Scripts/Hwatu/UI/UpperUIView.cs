using System;
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
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text subText;
        [SerializeField] private Image handRankIcon;

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

            if (subText == null)
            {
                throw new InvalidOperationException("Upper UI subtext is not assigned.");
            }
        }
    }
}
