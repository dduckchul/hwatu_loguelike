using System.Collections;
using TMPro;
using UnityEngine;

namespace Hwatu.UI
{
    [DisallowMultipleComponent]
    public sealed class EnemyOpeningDialogueView : MonoBehaviour
    {
        [SerializeField] private GameObject dialogueRoot;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField, Min(0f)] private float openingDelay = 1f;
        [SerializeField, Min(0f)] private float displayDuration = 3f;

        private Coroutine hideRoutine;

        private void Awake()
        {
            if (dialogueRoot != null)
            {
                dialogueRoot.SetActive(false);
            }
        }

        private IEnumerator Start()
        {
            if (openingDelay > 0f)
            {
                yield return new WaitForSeconds(openingDelay);
            }

            Show();
        }

        public void Show()
        {
            if (dialogueRoot == null || dialogueText == null || string.IsNullOrWhiteSpace(dialogueText.text))
            {
                return;
            }

            if (hideRoutine != null)
            {
                StopCoroutine(hideRoutine);
            }

            dialogueRoot.SetActive(true);
            hideRoutine = StartCoroutine(HideAfterDelay());
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(displayDuration);
            dialogueRoot.SetActive(false);
            hideRoutine = null;
        }
    }
}
