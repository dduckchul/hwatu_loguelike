using Hwatu.Cards;

namespace Hwatu.UI
{
    public static class CardTypeDisplayName
    {
        public static string Get(CardType cardType)
        {
            switch (cardType)
            {
                case CardType.Normal:
                    return "일반";
                case CardType.Bright:
                    return "광";
                case CardType.Ribbon:
                    return "띠";
                case CardType.Animal:
                    return "열끗";
                default:
                    return cardType.ToString();
            }
        }
    }
}
