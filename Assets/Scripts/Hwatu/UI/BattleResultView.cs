using System;
using System.Collections.Generic;
using System.Text;
using Hwatu.Hands;
using TMPro;
using UnityEngine;

namespace Hwatu.UI
{
    [DisallowMultipleComponent]
    public sealed class BattleResultView : MonoBehaviour
    {
        [SerializeField] private TMP_Text resultText;

        public void ShowPlayerResults(IReadOnlyList<HandComparisonResult> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            if (results.Count == 0)
            {
                throw new ArgumentException("At least one hand comparison result is required.", nameof(results));
            }

            ValidateReferences();

            if (results.Count == 1)
            {
                resultText.text = GetPlayerResultName(results[0]);
                return;
            }

            var builder = new StringBuilder();
            for (int index = 0; index < results.Count; index++)
            {
                if (index > 0)
                {
                    builder.AppendLine();
                }

                builder.Append("적 ");
                builder.Append(index + 1);
                builder.Append(" 상대: ");
                builder.Append(GetPlayerResultName(results[index]));
            }

            resultText.text = builder.ToString();
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
        }
    }
}
