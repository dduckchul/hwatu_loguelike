using System;
using UnityEngine;

namespace Hwatu.UI
{
    [DisallowMultipleComponent]
    public sealed class StoreView : MonoBehaviour
    {
        public event Action UpgradeRequested;
        public event Action RemovalRequested;
        public event Action SkipRequested;

        public bool IsOpen => gameObject.activeSelf;

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void RequestUpgrade()
        {
            UpgradeRequested?.Invoke();
        }

        public void RequestRemoval()
        {
            RemovalRequested?.Invoke();
        }

        public void RequestSkip()
        {
            SkipRequested?.Invoke();
        }
    }
}
