using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class IceShardProjectileBehavior : CameraSpaceProjectile
    {
        [SerializeField] BoxCollider2D boxCollider;

        public override Rect GetBounds()
        {
            return new Rect(boxCollider.bounds.center, transform.rotation * boxCollider.size);
        }

        public void SetData(float size, float damageMultiplier, float speed)
        {
            transform.localScale = Vector3.one * size * PlayerBehavior.Player.SizeMultiplier;

            DamageMultiplier = damageMultiplier;

            Speed = speed;
        }

        public override void Update()
        {
            base.Update();

            transform.rotation = Quaternion.FromToRotation(Vector3.up, Direction);
        }
    }
}