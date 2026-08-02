using System;

namespace Hwatu.Cards
{
    public sealed class CardInstance
    {
        public CardDefinition Definition { get; }
        public int UpgradeLevel { get; private set; }

        public CardInstance(CardDefinition definition, int upgradeLevel = 0)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));

            if (upgradeLevel < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(upgradeLevel),
                    upgradeLevel,
                    "Upgrade level cannot be negative.");
            }

            UpgradeLevel = upgradeLevel;
        }

        public void IncreaseUpgradeLevel()
        {
            UpgradeLevel++;
        }
    }
}
