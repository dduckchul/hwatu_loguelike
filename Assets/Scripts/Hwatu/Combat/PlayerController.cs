using UnityEngine;

namespace Hwatu.Combat
{
    [DisallowMultipleComponent]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField, Min(0)] private int startingMoney;

        public CharacterState State { get; private set; }
        public bool IsInitialized => State != null;

        public void InitializeForRun()
        {
            State = new CharacterState(startingMoney);
        }
    }
}
