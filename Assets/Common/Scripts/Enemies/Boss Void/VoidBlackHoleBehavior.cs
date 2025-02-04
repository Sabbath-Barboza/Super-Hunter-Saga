using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using OctoberStudio.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class VoidBlackHoleBehavior : MonoBehaviour
    {
        private static readonly int HIDE_TRIGGER = Animator.StringToHash("Hide");

        [SerializeField] Animator animator;
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] ParticleSystem chargeParticle;

        [SerializeField] float projectileDamage = 10f;

        private Coroutine damageCoroutine;

        public float Damage { get; set; }

        private bool IsHiding { get; set; }

        private List<SimpleEnemyProjectileBehavior> projectiles = new List<SimpleEnemyProjectileBehavior>();

        public void Hide() 
        {
            animator.SetTrigger(HIDE_TRIGGER);

            IsHiding = true;
        }

        public void OnHidden()
        {
            if(IsHiding)
            {
                IsHiding = false;
                gameObject.SetActive(false);
            }
        }

        public void Charge(PoolComponent<SimpleEnemyProjectileBehavior> projectilePool)
        {
            chargeParticle.Play();

            EasingManager.DoAfter(0.6f, () =>
            {
                for(int i = 0; i < 10; i++)
                {
                    var angle = 360f / 10 * i;
                    var projectile = projectilePool.GetEntity();
                    projectile.onFinished += OnProjectileFinished;
                    projectile.Damage = projectileDamage * StageController.Stage.EnemyDamage;
                    projectiles.Add(projectile);
                    projectile.Init(transform.position, Quaternion.Euler(0, 0, angle) * Vector2.up);
                } 
            });
        }

        private void OnProjectileFinished(SimpleEnemyProjectileBehavior projectile)
        {
            projectile.onFinished -= OnProjectileFinished;
            projectiles.Remove(projectile);
        }

        private IEnumerator DamageCoroutine()
        {
            while(true)
            {
                yield return new WaitForSeconds(1f);

                PlayerBehavior.Player.TakeDamage(Damage);
            }
        }

        public bool Intersects(VoidBlackHoleBehavior other)
        {
            return spriteRenderer.bounds.Intersects(other.spriteRenderer.bounds);
        }

        public bool Contains(Vector2 position)
        {
            Vector3 position3d = ((Vector3)position).SetZ(spriteRenderer.bounds.center.z);
            return spriteRenderer.bounds.Contains(position3d);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.GetComponent<PlayerBehavior>() != null)
            {
                damageCoroutine = StartCoroutine(DamageCoroutine());
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.GetComponent<PlayerBehavior>() != null)
            {
                StopCoroutine(damageCoroutine);
            }
        }

        public void Clear()
        {
            for(int i = 0; i < projectiles.Count; i++)
            {
                projectiles[i].onFinished -= OnProjectileFinished;
                projectiles[i].Disable();
            }

            projectiles.Clear();
            gameObject.SetActive(false);
        }
    }
}