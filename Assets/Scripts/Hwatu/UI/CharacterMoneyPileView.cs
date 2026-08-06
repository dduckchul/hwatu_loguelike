using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Hwatu.UI
{
    [DisallowMultipleComponent]
    public sealed class CharacterMoneyPileView : MonoBehaviour
    {
        [Serializable]
        private sealed class CoinDenomination
        {
            [SerializeField] private Sprite sprite;
            [SerializeField, Min(1)] private int value = 1;
            [SerializeField] private Color tint = Color.white;

            public Sprite Sprite => sprite;
            public int Value => value;
            public Color Tint => tint;
        }

        [Header("References")]
        [SerializeField] private TMP_Text moneyText;
        [SerializeField]
        private List<CoinDenomination> coinDenominations = new List<CoinDenomination>();

        [Header("Display")]
        [SerializeField, Range(1, 24)] private int maxVisibleCoins = 16;
        [SerializeField, Min(0f)] private float horizontalSpacing = 0.32f;
        [SerializeField, Min(0.01f)] private float coinScale = 3.2f;
        [SerializeField] private int baseSortingOrder = 20;

        private readonly List<SpriteRenderer> coinRenderers = new List<SpriteRenderer>();
        private readonly List<CoinDenomination> displayedCoins = new List<CoinDenomination>();

        private void Awake()
        {
            ValidateConfiguration();
            BuildCoinPool();
        }

        public void Show(int money)
        {
            if (money < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(money),
                    money,
                    "Displayed money cannot be negative.");
            }

            BuildDisplayedCoins(money);
            float startX = -(displayedCoins.Count - 1) * horizontalSpacing * 0.5f;

            for (int index = 0; index < coinRenderers.Count; index++)
            {
                SpriteRenderer renderer = coinRenderers[index];
                bool isVisible = index < displayedCoins.Count;
                renderer.enabled = isVisible;
                if (!isVisible)
                {
                    continue;
                }

                CoinDenomination denomination = displayedCoins[index];
                renderer.sprite = denomination.Sprite;
                renderer.color = denomination.Tint;
                renderer.transform.localPosition = new Vector3(
                    startX + (index * horizontalSpacing),
                    0f,
                    0f);
            }

            moneyText.text = $"{money}전";
        }

        private void BuildDisplayedCoins(int money)
        {
            displayedCoins.Clear();
            int remainingMoney = money;

            for (int index = coinDenominations.Count - 1;
                 index >= 0 && displayedCoins.Count < maxVisibleCoins;
                 index--)
            {
                CoinDenomination denomination = coinDenominations[index];
                int coinCount = remainingMoney / denomination.Value;
                remainingMoney %= denomination.Value;

                for (int coinIndex = 0;
                     coinIndex < coinCount && displayedCoins.Count < maxVisibleCoins;
                     coinIndex++)
                {
                    displayedCoins.Add(denomination);
                }
            }
        }

        private void BuildCoinPool()
        {
            for (int index = 0; index < maxVisibleCoins; index++)
            {
                var coinObject = new GameObject($"Coin_{index:00}");
                coinObject.transform.SetParent(transform, false);
                coinObject.transform.localScale = new Vector3(coinScale, coinScale, 1f);

                SpriteRenderer renderer = coinObject.AddComponent<SpriteRenderer>();
                renderer.sortingOrder = baseSortingOrder + index;
                renderer.enabled = false;
                coinRenderers.Add(renderer);
            }
        }

        private void ValidateConfiguration()
        {
            if (moneyText == null)
            {
                throw new InvalidOperationException("Money text is not assigned.");
            }

            if (coinDenominations == null || coinDenominations.Count == 0)
            {
                throw new InvalidOperationException(
                    "At least one coin denomination must be assigned.");
            }

            int previousValue = 0;
            for (int index = 0; index < coinDenominations.Count; index++)
            {
                CoinDenomination denomination = coinDenominations[index];
                if (denomination == null || denomination.Sprite == null)
                {
                    throw new InvalidOperationException(
                        $"Coin denomination at index {index} is not assigned.");
                }

                if (denomination.Value <= previousValue)
                {
                    throw new InvalidOperationException(
                        "Coin denominations must be ordered by increasing value.");
                }

                previousValue = denomination.Value;
            }

            if (coinDenominations[0].Value != 1)
            {
                throw new InvalidOperationException(
                    "The smallest coin denomination must be worth 1 money.");
            }
        }
    }
}
