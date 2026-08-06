using System;

namespace Hwatu.Combat
{
    public sealed class CharacterState
    {
        public int Money { get; private set; }
        public bool IsDefeated => Money <= 0;

        public CharacterState(int startingMoney)
        {
            if (startingMoney < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(startingMoney),
                    startingMoney,
                    "Starting money cannot be negative.");
            }

            Money = startingMoney;
        }

        public int TransferMoneyTo(CharacterState target, int requestedAmount)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (ReferenceEquals(this, target))
            {
                throw new InvalidOperationException("A character cannot transfer money to itself.");
            }

            if (requestedAmount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(requestedAmount),
                    requestedAmount,
                    "Requested amount cannot be negative.");
            }

            int transferredAmount = Math.Min(Money, requestedAmount);
            Money -= transferredAmount;
            target.Money = checked(target.Money + transferredAmount);
            return transferredAmount;
        }
    }
}
