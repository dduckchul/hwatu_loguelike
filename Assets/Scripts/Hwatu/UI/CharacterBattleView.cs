using System;
using System.Collections;
using UnityEngine;

namespace Hwatu.UI
{
    [DisallowMultipleComponent]
    public sealed class CharacterBattleView : MonoBehaviour
    {
        [Header("Renderer")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Transform motionTarget;

        [Header("Sprites")]
        [SerializeField] private Sprite idleSprite;
        [SerializeField] private Sprite idleSprite2;
        [SerializeField] private Sprite hitSprite;
        [SerializeField, Min(0.01f)] private float idleFrameDuration = 0.5f;

        [Header("Attack Motion")]
        [SerializeField] private Vector3 attackOffset;
        [SerializeField, Min(0f)] private float attackWindupDistance = 0.2f;
        [SerializeField, Min(0.01f)] private float attackWindupDuration = 0.12f;
        [SerializeField, Min(0f)] private float attackWindupHoldDuration = 0.5f;
        [SerializeField, Min(0.01f)] private float attackForwardDuration = 0.15f;
        [SerializeField, Min(0.01f)] private float attackReturnDuration = 0.15f;

        [Header("Hit Motion")]
        [SerializeField] private ParticleSystem hitEffect;
        [SerializeField] private Vector3 hitOffset;
        [SerializeField, Min(0f)] private float shakeDistance = 0.08f;
        [SerializeField, Range(1, 6)] private int shakeCount = 3;
        [SerializeField, Min(0.01f)] private float hitDuration = 0.6f;
        [SerializeField] private Color hitFlashColor = Color.red;

        private Vector3 restingLocalPosition;
        private Color restingColor;
        private Coroutine idleAnimation;
        private bool isInitialized;

        private void Awake()
        {
            ValidateReferences();
            restingLocalPosition = motionTarget.localPosition;
            restingColor = GetColor();
            isInitialized = true;
            if (hitEffect != null)
            {
                StopHitEffect();
            }

            ShowIdle();
        }

        public void ShowIdle()
        {
            StopIdleAnimation();
            SetSprite(idleSprite);

            if (isActiveAndEnabled)
            {
                idleAnimation = StartCoroutine(PlayIdleAnimation());
            }
        }

        public void ShowShowdown()
        {
            StopIdleAnimation();
            SetSprite(idleSprite2);
        }

        public IEnumerator PlayAttackWindup()
        {
            Vector3 windupOffset = attackOffset.sqrMagnitude > 0f
                ? -attackOffset.normalized * attackWindupDistance
                : Vector3.zero;

            yield return MoveTo(
                restingLocalPosition + windupOffset,
                attackWindupDuration);

            if (attackWindupHoldDuration > 0f)
            {
                yield return new WaitForSeconds(attackWindupHoldDuration);
            }
        }

        public IEnumerator PlayAttackForward()
        {
            yield return MoveTo(
                restingLocalPosition + attackOffset,
                attackForwardDuration);
        }

        public IEnumerator PlayAttackReturn()
        {
            yield return MoveTo(restingLocalPosition, attackReturnDuration);
            ShowShowdown();
        }

        public IEnumerator PlayHit()
        {
            Vector3 startPosition = restingLocalPosition;
            StopIdleAnimation();
            SetSprite(hitSprite);
            PlayHitEffect();
            SetColor(hitFlashColor);

            float elapsed = 0f;
            while (elapsed < hitDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / hitDuration);
                float impactCurve = Mathf.Sin(progress * Mathf.PI);
                float shakeCurve = Mathf.Sin(progress * Mathf.PI * 2f * shakeCount)
                    * (1f - progress);
                Vector3 shakeOffset = Vector3.right * (shakeDistance * shakeCurve);

                motionTarget.localPosition = startPosition
                    + (hitOffset * impactCurve)
                    + shakeOffset;

                yield return null;
            }

            motionTarget.localPosition = restingLocalPosition;
            SetColor(restingColor);
        }

        private IEnumerator MoveTo(Vector3 targetPosition, float duration)
        {
            Vector3 startPosition = motionTarget.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                motionTarget.localPosition = Vector3.Lerp(startPosition, targetPosition, progress);
                yield return null;
            }

            motionTarget.localPosition = targetPosition;
        }

        private IEnumerator PlayIdleAnimation()
        {
            var frameWait = new WaitForSeconds(idleFrameDuration);

            while (true)
            {
                yield return frameWait;
                SetSprite(idleSprite2);
                yield return frameWait;
                SetSprite(idleSprite);
            }
        }

        private void StopIdleAnimation()
        {
            if (idleAnimation == null)
            {
                return;
            }

            StopCoroutine(idleAnimation);
            idleAnimation = null;
        }

        private void OnDisable()
        {
            StopIdleAnimation();

            if (!isInitialized)
            {
                return;
            }

            if (motionTarget != null)
            {
                motionTarget.localPosition = restingLocalPosition;
            }

            if (spriteRenderer != null)
            {
                SetColor(restingColor);
            }

            if (hitEffect != null)
            {
                StopHitEffect();
            }
        }

        private void PlayHitEffect()
        {
            if (hitEffect == null)
            {
                return;
            }

            StopHitEffect();
            hitEffect.Play(withChildren: true);
        }

        private void StopHitEffect()
        {
            hitEffect.Stop(
                withChildren: true,
                stopBehavior: ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void SetSprite(Sprite sprite)
        {
            if (sprite == null)
            {
                throw new InvalidOperationException("Character battle sprite is not assigned.");
            }

            spriteRenderer.sprite = sprite;
        }

        private Color GetColor()
        {
            return spriteRenderer.color;
        }

        private void SetColor(Color color)
        {
            spriteRenderer.color = color;
        }

        private void ValidateReferences()
        {
            if (spriteRenderer == null)
            {
                throw new InvalidOperationException("Character sprite renderer is not assigned.");
            }

            if (motionTarget == null)
            {
                throw new InvalidOperationException("Character motion target is not assigned.");
            }

            if (idleSprite == null || idleSprite2 == null || hitSprite == null)
            {
                throw new InvalidOperationException("Idle, Idle2, and Attack sprites must all be assigned.");
            }

            if (idleFrameDuration <= 0f)
            {
                throw new InvalidOperationException(
                    "Idle frame duration must be greater than zero.");
            }
        }
    }
}
