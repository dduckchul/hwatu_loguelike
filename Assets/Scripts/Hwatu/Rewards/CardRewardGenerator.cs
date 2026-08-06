using System;
using System.Collections.Generic;
using Hwatu.Cards;
using Hwatu.Randomness;

namespace Hwatu.Rewards
{
    public sealed class CardRewardGenerator
    {
        public IReadOnlyList<CardData> GenerateNormalRewards(
            IReadOnlyList<CardData> cardPool,
            int rewardCount,
            IRandomSource randomSource)
        {
            if (cardPool == null)
            {
                throw new ArgumentNullException(nameof(cardPool));
            }

            if (rewardCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rewardCount));
            }

            if (randomSource == null)
            {
                throw new ArgumentNullException(nameof(randomSource));
            }

            var candidates = new List<CardData>();
            var includedCardIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (CardData cardData in cardPool)
            {
                if (cardData == null)
                {
                    throw new InvalidOperationException("Reward card pool cannot contain a null card.");
                }

                if (cardData.CardType != CardType.Normal)
                {
                    continue;
                }

                if (!includedCardIds.Add(cardData.CardId))
                {
                    throw new InvalidOperationException(
                        $"Reward card pool contains duplicate ID '{cardData.CardId}'.");
                }

                candidates.Add(cardData);
            }

            if (candidates.Count < rewardCount)
            {
                throw new InvalidOperationException(
                    $"Reward card pool requires at least {rewardCount} unique Normal cards.");
            }

            for (int index = 0; index < rewardCount; index++)
            {
                int selectedIndex = randomSource.Next(index, candidates.Count);
                CardData selectedCard = candidates[selectedIndex];
                candidates[selectedIndex] = candidates[index];
                candidates[index] = selectedCard;
            }

            return candidates.GetRange(0, rewardCount);
        }
    }
}
