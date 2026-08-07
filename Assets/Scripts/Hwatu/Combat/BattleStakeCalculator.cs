using System;

namespace Hwatu.Combat
{
    public sealed class BattleStakeCalculator
    {
        public const int StakePerBattle = 5;

        public int Calculate(int battleNumber)
        {
            if (battleNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(battleNumber),
                    battleNumber,
                    "Battle number must be greater than zero.");
            }

            return checked(battleNumber * StakePerBattle);
        }
    }
}
