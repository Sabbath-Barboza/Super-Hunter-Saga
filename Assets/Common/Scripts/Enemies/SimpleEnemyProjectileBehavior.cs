using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio
{
    public class SimpleEnemyProjectileBehavior : MonoBehaviour
    {
        [SerializeField] float speed;
        [Tooltip("After this amount of time the projectile will get disabled")]
        [SerializeField] float lifetime;
        [Tooltip("Should the projectile pass through the player or hit him and get disabled")]
        [SerializeField] bool hideOnHit = true;

        [Space]
        [Tooltip("Reference for clearing the trail on Disable. Can be null")]
        [SerializeField] TrailRenderer trail;

        private Vector3 direction;
        private float endTime;

        public bool IsActive { get; protected set; }
        public float LifeTime { get; set; }
        public float Damage { get; set; }
        public float Speed => speed;

        public event UnityAction<SimpleEnemyProjectileBehavior> onFinished;

        private void Awake()
        {
            LifeTime = lifetime;
        }

        public virtual void Init(Vector2 position, Vector2 direction)
        {
            transform.position = position;
            this.direction = direction;

            endTime = Time.time + LifeTime;

            IsActive = true;
        }

        protected virtual void Update()
        {
            if (!IsActive) return;

            if(Time.time > endTime)
            {
                Disable();
                return;
            }

            if(speed > 0)
            {
                transform.position += direction * Time.deltaTime * speed;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (hideOnHit)
            {
                SuccessfulHit();
            }
        }

        protected virtual void SuccessfulHit()
        {
            Disable();
        }

        public virtual void Disable()
        {
            if(trail != null) trail.Clear();
            gameObject.SetActive(false);
            onFinished?.Invoke(this);
        }
    }
}