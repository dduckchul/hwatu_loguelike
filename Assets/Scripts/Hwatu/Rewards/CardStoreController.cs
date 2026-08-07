using System;
using Hwatu.Cards;
using Hwatu.Combat;
using Hwatu.Deck;
using Hwatu.Randomness;
using Hwatu.UI;
using UnityEngine;

namespace Hwatu.Rewards
{
    [DisallowMultipleComponent]
    public sealed class CardStoreController : MonoBehaviour
    {
        [SerializeField] private PlayerDeckInitializer playerDeckInitializer;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private CardCatalogData cardCatalog;
        [SerializeField] private CardStoreView cardStoreView;
        [SerializeField] private RunRandomProvider runRandomProvider;

        private readonly CardRewardGenerator rewardGenerator = new CardRewardGenerator();
        private readonly StoreCardPriceCalculator priceCalculator =
            new StoreCardPriceCalculator();

        private int purchasedCardCount;

        public bool IsOpen { get; private set; }
        public int NextCardPrice => priceCalculator.Calculate(purchasedCardCount);

        private void Awake()
        {
            ValidateReferences();
            cardStoreView.Hide();
        }

        private void OnEnable()
        {
            if (cardStoreView != null)
            {
                cardStoreView.PurchaseRequested += HandleCardPurchaseRequested;
            }
        }

        private void OnDisable()
        {
            if (cardStoreView != null)
            {
                cardStoreView.PurchaseRequested -= HandleCardPurchaseRequested;
                cardStoreView.Hide();
            }

            IsOpen = false;
        }

        public void Open()
        {
            if (IsOpen)
            {
                return;
            }

            ValidateReferences();
            if (playerDeckInitializer.Deck == null)
            {
                throw new InvalidOperationException("Player deck is not initialized.");
            }

            purchasedCardCount = 0;
            IsOpen = true;
            cardStoreView.Show(
                rewardGenerator.GenerateNormalRewards(
                    cardCatalog.Cards,
                    cardStoreView.CardSlotCount,
                    runRandomProvider.GetStream(RandomStreamId.CardReward)));
        }

        public void Close()
        {
            if (cardStoreView != null)
            {
                cardStoreView.Hide();
            }

            IsOpen = false;
        }

        private void HandleCardPurchaseRequested(CardData cardData)
        {
            if (cardData == null)
            {
                throw new ArgumentNullException(nameof(cardData));
            }

            if (!IsOpen)
            {
                return;
            }

            int price = NextCardPrice;
            if (!playerController.TrySpendMoney(price))
            {
                return;
            }

            playerDeckInitializer.Deck.AddCard(
                new CardInstance(cardData.ToDefinition()));
            cardStoreView.MarkPurchased(cardData);
            purchasedCardCount = checked(purchasedCardCount + 1);
        }

        private void ValidateReferences()
        {
            if (playerDeckInitializer == null)
            {
                throw new InvalidOperationException(
                    "Player deck initializer is not assigned.");
            }

            if (playerController == null)
            {
                throw new InvalidOperationException(
                    "Player controller is not assigned.");
            }

            if (cardCatalog == null)
            {
                throw new InvalidOperationException(
                    "Card catalog is not assigned.");
            }

            if (cardStoreView == null)
            {
                throw new InvalidOperationException(
                    "Card store view is not assigned.");
            }

            if (runRandomProvider == null)
            {
                throw new InvalidOperationException(
                    "Run random provider is not assigned.");
            }
        }
    }
}
