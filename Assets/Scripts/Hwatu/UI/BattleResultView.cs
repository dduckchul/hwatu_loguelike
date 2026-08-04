using System;
using Hwatu.Hands;
using TMPro;
using UnityEngine;

namespace Hwatu.UI
{
    [DisallowMultipleComponent]
    public sealed class BattleResultView : MonoBehaviour
    {
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private GameObject resultRoot;

        public void ShowPlayerResult(
            HandComparisonResult result,
            int enemyIndex,
            int enemyCount)
        {
            if (enemyCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(enemyCount));
            }

            if (enemyIndex < 0 || enemyIndex >= enemyCount)
            {
                throw new ArgumentOutOfRangeException(nameof(enemyIndex));
            }

            ValidateReferences();
            string resultName = GetPlayerResultName(result);

            resultText.text = enemyCount == 1
                ? resultName
                : $"적 {enemyIndex + 1} 상대: {resultName}";
            resultRoot.SetActive(true);
        }

        public void Hide()
        {
            ValidateReferences();
            resultRoot.SetActive(false);
        }

        private static string GetPlayerResultName(HandComparisonResult result)
        {
            switch (result)
            {
                case HandComparisonResult.FirstWins:
                    return "승리";
                case HandComparisonResult.Draw:
                    return "무승부";
                case HandComparisonResult.SecondWins:
                    return "패배";
                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, null);
            }
        }

        private void ValidateReferences()
        {
            if (resultText == null)
            {
                throw new InvalidOperationException("Battle result text is not assigned.");
            }

            if (resultRoot == null)
            {
                throw new InvalidOperationException("Battle result root is not assigned.");
            }
        }
    }
}
