using System;
using Hwatu.Cards;
using Hwatu.Deck;
using Hwatu.Randomness;
using Hwatu.UI;
using UnityEngine;

namespace Hwatu.Rewards
{
    [DisallowMultipleComponent]
    public sealed class CardRewardController : MonoBehaviour
    {
        private const int RewardCount = 3;

        [SerializeField] private PlayerDeckInitializer playerDeckInitializer;
        [SerializeField] private CardCatalogData cardCatalog;
        [SerializeField] private CardRewardView rewardView;
        [SerializeField] private RunRandomProvider runRandomProvider;

        private readonly CardRewardGenerator rewardGenerator = new CardRewardGenerator();

        public event Action RewardCompleted;
        public bool IsOpen => rewardView != null && rewardView.IsOpen;

        private void Awake()
        {
            ValidateReferences();
            rewardView.Hide();
        }

        private void OnEnable()
        {
            if (rewardView == null)
            {
                return;
            }

            rewardView.RewardConfirmed += HandleRewardConfirmed;
            rewardView.RewardSkipped += HandleRewardSkipped;
        }

        private void OnDisable()
        {
            if (rewardView == null)
            {
                return;
            }

            rewardView.RewardConfirmed -= HandleRewardConfirmed;
            rewardView.RewardSkipped -= HandleRewardSkipped;
        }

        public void ShowRewards()
        {
            ValidateReferences();

            if (IsOpen)
            {
                throw new InvalidOperationException("Card rewards are already open.");
            }

            if (playerDeckInitializer.Deck == null)
            {
                throw new InvalidOperationException("Player deck is not initialized.");
            }

            rewardView.Show(
                rewardGenerator.GenerateNormalRewards(
                    cardCatalog.Cards,
                    RewardCount,
                    runRandomProvider.GetStream(RandomStreamId.CardReward)));
        }

        private void HandleRewardConfirmed(CardData cardData)
        {
            if (cardData == null)
            {
                throw new ArgumentNullException(nameof(cardData));
            }

            playerDeckInitializer.Deck.AddCard(
                new CardInstance(cardData.ToDefinition()));
            CompleteReward();
        }

        private void HandleRewardSkipped()
        {
            CompleteReward();
        }

        private void CompleteReward()
        {
            rewardView.Hide();
            RewardCompleted?.Invoke();
        }

        private void ValidateReferences()
        {
            if (playerDeckInitializer == null)
            {
                throw new InvalidOperationException("Player deck initializer is not assigned.");
            }

            if (cardCatalog == null)
            {
                throw new InvalidOperationException("Card catalog is not assigned.");
            }

            if (rewardView == null)
            {
                throw new InvalidOperationException("Card reward view is not assigned.");
            }

            if (runRandomProvider == null)
            {
                throw new InvalidOperationException("Run random provider is not assigned.");
            }
        }
    }
}
