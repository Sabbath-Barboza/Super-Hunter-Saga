using OctoberStudio.Easing;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio
{
    public class SimplePlayerProjectileBehavior : ProjectileBehavior
    {
        [SerializeField] float speed;
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] TrailRenderer trail;
        [SerializeField] float lifetime = -1;
        [SerializeField] Transform rotatingPart;
        [SerializeField] bool selfDestructOnHit = true;
        [SerializeField] List<ParticleSystem> particles;

        private Vector3 direction;
        private float spawnTime;

        public float Speed { get; set; }
        public float LifeTime { get; set; }

        IEasingCoroutine scaleCoroutine;

        public UnityAction<SimplePlayerProjectileBehavior> onFinished;

        public void Init(Vector2 position, Vector2 direction)
        {
            Init();

            transform.position = position;
            transform.localScale = Vector3.one * PlayerBehavior.Player.SizeMultiplier;
            this.direction = direction;

            for (int i = 0; i < particles.Count; i++)
            {
                particles[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particles[i].Clear();
                particles[i].Play();
            }

            if (rotatingPart != null) rotatingPart.rotation = Quaternion.FromToRotation(Vector2.up, direction);

            Speed = speed * PlayerBehavior.Player.ProjectileSpeedMultiplier;

            spawnTime = Time.time;

            LifeTime = lifetime * PlayerBehavior.Player.DurationMultiplier;

            if (trail != null) trail.Clear();
        }

        private void Update()
        {
            if (spriteRenderer != null && !spriteRenderer.isVisible) Clear();

            transform.position += direction * Time.deltaTime * Speed;

            if (LifeTime > 0)
            {
                if(Time.time - spawnTime > LifeTime)
                {
                    Clear();

                    onFinished?.Invoke(this);
                }
            }
        }

        public IEasingCoroutine ScaleRotatingPart(Vector3 initialScale, Vector3 targetScale)
        {
            if (rotatingPart == null) return null;

            rotatingPart.localScale = initialScale;

            scaleCoroutine = rotatingPart.DoLocalScale(targetScale, 0.25f);

            return scaleCoroutine;
        }

        public void Clear()
        {
            for (int i = 0; i < particles.Count; i++)
            {
                particles[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particles[i].Clear();
            }

            scaleCoroutine.StopIfExists();
            gameObject.SetActive(false);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (selfDestructOnHit)
            {
                Clear();

                onFinished?.Invoke(this);
            }
        }
    }
}