using UnityEngine;

namespace Hwatu.Cards
{
    [CreateAssetMenu(fileName = "CardData", menuName = "Hwatu/Card")]
    public sealed class CardData : ScriptableObject
    {
        [SerializeField] private string cardId;
        [SerializeField, Range(CardDefinition.MinMonth, CardDefinition.MaxMonth)]
        private int month = CardDefinition.MinMonth;
        [SerializeField] private CardType cardType;
        [SerializeField] private Sprite artwork;

        public string CardId => cardId;
        public int Month => month;
        public CardType CardType => cardType;
        public Sprite Artwork => artwork;

        public CardDefinition ToDefinition()
        {
            return new CardDefinition(cardId, month, cardType);
        }

        private void OnValidate()
        {
            if (cardId != null)
            {
                cardId = cardId.Trim();
            }
        }
    }
}
