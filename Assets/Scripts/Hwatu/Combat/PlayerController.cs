using System;
using Hwatu.UI;
using UnityEngine;

namespace Hwatu.Combat
{
    [DisallowMultipleComponent]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField, Min(0)] private int startingMoney;
        [SerializeField] private CharacterBattleView battleView;
        [SerializeField] private CharacterMoneyPileView moneyView;

        public CharacterState State { get; private set; }
        public bool IsInitialized => State != null;
        public CharacterBattleView BattleView => battleView;

        public event Action<int> MoneyChanged;

        public void InitializeForRun()
        {
            if (moneyView == null)
            {
                throw new InvalidOperationException("Player money view is not assigned.");
            }

            State = new CharacterState(startingMoney);
            RefreshMoneyView();
        }

        public void RefreshMoneyView()
        {
            if (State == null)
            {
                throw new InvalidOperationException("Player is not initialized for the run.");
            }

            moneyView.Show(State.Money);
            MoneyChanged?.Invoke(State.Money);
        }

        public bool TrySpendMoney(int amount)
        {
            if (State == null)
            {
                throw new InvalidOperationException(
                    "Player is not initialized for the run.");
            }

            if (!State.TrySpendMoney(amount))
            {
                return false;
            }

            RefreshMoneyView();
            return true;
        }
    }
}
