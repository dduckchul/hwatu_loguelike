using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hwatu.Intro
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class PersistentIntroMusic : MonoBehaviour
    {
        private const string TitleSceneName = "TitleScene";

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
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;

            if (instance == this)
            {
                instance = null;
            }
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene.name == TitleSceneName)
            {
                Destroy(gameObject);
            }
        }
    }
}
