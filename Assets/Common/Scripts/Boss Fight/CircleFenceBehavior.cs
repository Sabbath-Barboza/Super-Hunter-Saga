using UnityEngine;

namespace OctoberStudio
{
    public class CircleFenceBehavior : BossFenceBehavior
    {
        [SerializeField] float radius = 5f;
        public float Radius { get; private set; }

        public override void Init()
        {
            Radius = radius;

            float perimeter = 2 * Mathf.PI * radius;
            FencePoolSize = Mathf.CeilToInt(perimeter / linkDistance);

            base.Init();
        }

        public override void SpawnFence(Vector2 center)
        {
            base.SpawnFence(center);

            float angleStep = 360f / FencePoolSize;

            for(int i = 0; i < FencePoolSize; i++)
            {
                var position = Center + (Vector2)(Quaternion.Euler(0, 0, angleStep * i) * Vector2.up * Radius);

                if (StageController.FieldManager.ValidatePosition(position, Vector2.zero, false))
                {
                    var link = fenceLinkPool.GetEntity();
                    link.transform.position = position;
                    link.transform.rotation = Quaternion.identity;

                    fenceLinks.Add(link);
                }
            }
        }

        public override Vector2 GetRandomPointInside(float offset)
        {
            return Center + Random.insideUnitCircle * (Radius - offset);
        }

        public override bool ValidatePosition(Vector2 position, Vector2 offset)
        {
            var biggerOffset = Mathf.Max(offset.x, offset.y);

            return Vector2.Distance(Center, position) < (Radius - biggerOffset);
        }

        public void SetRadiusOverride(float radiusOverride)
        {
            Radius = radiusOverride;

            float perimeter = 2 * Mathf.PI * Radius;
            FencePoolSize = Mathf.CeilToInt(perimeter / linkDistance);
        }

        public void ResetRadiusOverride()
        {
            Radius = radius;

            float perimeter = 2 * Mathf.PI * Radius;
            FencePoolSize = Mathf.CeilToInt(perimeter / linkDistance);
        }

        public override void RemoveFence()
        {
            base.RemoveFence();

            ResetRadiusOverride();
        }
    }
}