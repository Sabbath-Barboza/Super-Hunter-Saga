using OctoberStudio.Easing;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio
{
    public class CharacterBehavior : MonoBehaviour
    {
        private static readonly int DEFEAT_TRIGGER = Animator.StringToHash("Defeat");
        private static readonly int REVIVE_TRIGGER = Animator.StringToHash("Revive");
        private static readonly int SPEED_FLOAT = Animator.StringToHash("Speed");

        protected static readonly int _Overlay = Shader.PropertyToID("_Overlay");

        [SerializeField] SpriteRenderer playerSpriteRenderer;
        [SerializeField] Animator animator;
        [SerializeField] Color hitColor;

        IEasingCoroutine damageCoroutine;

        public void SetSpeed(float speed)
        {
            animator.SetFloat(SPEED_FLOAT, speed);
        }

        public void SetLocalScale(Vector3 scale)
        {
            transform.localScale = scale;
        }

        public void PlayReviveAnimation()
        {
            animator.SetTrigger(REVIVE_TRIGGER);
        }

        public void PlayDefeatAnimation()
        {
            animator.SetTrigger(DEFEAT_TRIGGER);
        }

        public void SetSortingOrder(int order) 
        {
            playerSpriteRenderer.sortingOrder = order;
        }

        public void FlashHit(UnityAction onFinish = null)
        {
            if (damageCoroutine.ExistsAndActive()) return;

            var transparentColor = hitColor;
            transparentColor.a = 0;

            playerSpriteRenderer.material.SetColor(_Overlay, transparentColor);

            damageCoroutine = playerSpriteRenderer.material.DoColor(_Overlay, hitColor, 0.05f).SetOnFinish(() =>
            {
                damageCoroutine = playerSpriteRenderer.material.DoColor(_Overlay, transparentColor, 0.05f).SetOnFinish(onFinish);
            });
        }
    }
}