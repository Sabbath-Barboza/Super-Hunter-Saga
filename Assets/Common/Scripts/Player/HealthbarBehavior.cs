using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using UnityEngine;

namespace OctoberStudio
{
    public class HealthbarBehavior : MonoBehaviour
    {
        [SerializeField] SpriteRenderer fillImage;
        [SerializeField] SpriteRenderer backgroundImage;
        [SerializeField] Transform maskTransform;
        [SerializeField] float maskMaxPosition;
        [SerializeField] float maskMaxScale;

        public float MaxHP { get; private set; }
        public float HP { get; private set; }

        public bool IsZero => HP <= 0;
        public bool IsMax => HP >= MaxHP;

        private bool autoShowOnChaned;
        private bool autoHideWhenMax;

        private IEasingCoroutine showHideCoroutine;

        private bool isShown = false;

        public void Init(float maxHP)
        {
            MaxHP = maxHP;
            HP = MaxHP;

            Redraw();
            
        }

        public void SetAutoShowOnChanged(bool value)
        {
            autoShowOnChaned = value;
        }

        public void SetAutoHideWhenMax(bool value)
        {
            autoHideWhenMax = value;
            if (HP == MaxHP) ForceHide();
        }

        public void AddHP(float value)
        {
            if(value < 0)
            {
                Subtract(-value);
                return;
            }

            HP += value;
            if (HP > MaxHP)
            {
                HP = MaxHP;
                if (autoHideWhenMax) Hide();
            }

            Redraw();
        }

        public void AddPercentage(float percent)
        {
            AddHP(MaxHP * percent / 100f);
        }

        public void Subtract(float value)
        {
            if (value < 0)
            {
                AddHP(-value);
                return;
            }

            HP -= value;

            if (HP <= 0)
            {
                HP = 0;
                Hide();
            }
            else
            {
                if (autoShowOnChaned && !isShown) Show();
                Redraw();
            }
        }

        public void ResetHP(float duration = 0)
        {
            if(duration > 0)
            {
                EasingManager.DoFloat(0, MaxHP, duration, (hp) =>
                {
                    HP = hp;
                    Redraw();
                });

                Show();
            } else
            {
                HP = MaxHP;
                Redraw();
            }
        }

        public void ChangeMaxHP(float newMaxHP, bool scaleHP = true)
        {
            var oldMaxHP = MaxHP;
            MaxHP = newMaxHP;

            if (scaleHP)
            {
                var scaleFactor = newMaxHP / oldMaxHP;

                var newHP = HP * scaleFactor;
                var difference = newHP - HP;

                AddHP(difference);
            } else
            {
                Redraw();
            }
            
        }

        public void Redraw()
        {
            float t = HP / MaxHP;
            maskTransform.localPosition = Vector3.left * maskMaxPosition * (1 - t);
            maskTransform.localScale = maskTransform.localScale.SetX(maskMaxScale * t);
        }

        public void Show()
        {
            Redraw();

            isShown = true;

            if (showHideCoroutine != null && showHideCoroutine.IsActive) showHideCoroutine.Stop();
            showHideCoroutine = new FloatEasingCoroutine(fillImage.color.a, 1f, 0.3f, 0, SetAlpha).SetEasing(EasingType.SineOut);
        }

        public void Hide()
        {
            isShown = false;

            if (showHideCoroutine != null && showHideCoroutine.IsActive) showHideCoroutine.Stop();
            showHideCoroutine = new FloatEasingCoroutine(fillImage.color.a, 0f, 0.3f, 0, SetAlpha).SetEasing(EasingType.SineOut);
        }

        public void ForceHide()
        {
            isShown = false;

            SetAlpha(0);
        }

        private void SetAlpha(float alpha)
        {
            if (fillImage != null) fillImage.SetAlpha(alpha);
            if (backgroundImage != null) backgroundImage.SetAlpha(alpha);
        }
    }
}