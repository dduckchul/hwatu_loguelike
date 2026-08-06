using System;

namespace Hwatu.Cards
{
    public sealed class CardDefinition
    {
        public const int MinMonth = 1;
        public const int MaxMonth = 10;

        public string Id { get; }
        public int Month { get; }
        public CardType CardType { get; }

        public CardDefinition(string id, int month, CardType cardType)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Card ID cannot be empty.", nameof(id));
            }

            if (month < MinMonth || month > MaxMonth)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(month),
                    month,
                    $"Card month must be between {MinMonth} and {MaxMonth}.");
            }

            if (!Enum.IsDefined(typeof(CardType), cardType))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cardType),
                    cardType,
                    "Card type is not defined.");
            }

            Id = id.Trim();
            Month = month;
            CardType = cardType;
        }
    }
}
