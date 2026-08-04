using System;

namespace Hwatu.Cards
{
    public sealed class CardInstance
    {
        public CardDefinition Definition { get; }

        public CardInstance(CardDefinition definition)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }
    }
}
