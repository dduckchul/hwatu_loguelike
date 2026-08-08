using UnityEngine;

namespace Hwatu.Intro
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class PersistentIntroMusic : MonoBehaviour
    {
        private static PersistentIntroMusic instance;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
