using OctoberStudio.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    public class RectFieldBehavior : IFieldBehavior
    {
        StageChunkBehavior chunk;

        List<Transform> borders = new List<Transform>();

        public void Init(StageFieldData stageFieldData)
        {
            chunk = Object.Instantiate(stageFieldData.BackgroundPrefab).GetComponent<StageChunkBehavior>();

            chunk.transform.position = Vector3.zero;
            chunk.transform.rotation = Quaternion.identity;
            chunk.transform.localScale = Vector3.one;

            if (stageFieldData.TopPrefab != null)
            {
                var topBorder = Object.Instantiate(stageFieldData.TopPrefab).GetComponent<Transform>();
                topBorder.transform.position = Vector3.up * chunk.Size.y;
                borders.Add(topBorder);
            }

            if (stageFieldData.BottomPrefab != null)
            {
                var bottomBorder = Object.Instantiate(stageFieldData.BottomPrefab).GetComponent<Transform>();
                bottomBorder.transform.position = Vector3.down * chunk.Size.y;
                borders.Add(bottomBorder);
            }

            if (stageFieldData.LeftPrefab != null)
            {
                var leftBorder = Object.Instantiate(stageFieldData.LeftPrefab).GetComponent<Transform>();
                leftBorder.transform.position = Vector3.left * chunk.Size.x;
                borders.Add(leftBorder);
            }

            if (stageFieldData.RightPrefab != null)
            {
                var rightBorder = Object.Instantiate(stageFieldData.RightPrefab).GetComponent<Transform>();
                rightBorder.transform.position = Vector3.right * chunk.Size.x;
                borders.Add(rightBorder);
            }

            if (stageFieldData.TopLeftPrefab != null)
            {
                var topLeftCorner = Object.Instantiate(stageFieldData.TopLeftPrefab).GetComponent<Transform>();
                topLeftCorner.transform.position = new Vector2(-chunk.Size.x, chunk.Size.y);
                borders.Add(topLeftCorner);
            }

            if (stageFieldData.TopRightPrefab != null)
            {
                var topRightCorner = Object.Instantiate(stageFieldData.TopRightPrefab).GetComponent<Transform>();
                topRightCorner.transform.position = new Vector2(chunk.Size.x, chunk.Size.y);
                borders.Add(topRightCorner);
            }

            if (stageFieldData.BottomLeftPrefab != null)
            {
                var bottomLeftCorner = Object.Instantiate(stageFieldData.BottomLeftPrefab).GetComponent<Transform>();
                bottomLeftCorner.transform.position = new Vector2(-chunk.Size.x, -chunk.Size.y);
                borders.Add(bottomLeftCorner);
            }

            if (stageFieldData.BottomRightPrefab != null)
            {
                var bottomRightCorner = Object.Instantiate(stageFieldData.BottomRightPrefab).GetComponent<Transform>();
                bottomRightCorner.transform.position = new Vector2(chunk.Size.x, -chunk.Size.y);
                borders.Add(bottomRightCorner);
            }   
        }

        public void Update()
        {

        }

        public bool ValidatePosition(Vector2 position)
        {
            if (position.x > chunk.transform.position.x + chunk.Size.x / 2) return false;
            if (position.x < chunk.transform.position.x - chunk.Size.x / 2) return false;

            if (position.y > chunk.transform.position.y + chunk.Size.y / 2) return false;
            if (position.y < chunk.transform.position.y - chunk.Size.y / 2) return false;

            return true;
        }

        public Vector2 GetRandomPositionOnBorder()
        {
            Vector2 randomPoint = Random.insideUnitCircle.normalized * Mathf.Max(chunk.Size.x, chunk.Size.y);

            if (randomPoint.x > chunk.Size.x / 2) randomPoint.x = chunk.Size.x / 2;
            if (randomPoint.x < -chunk.Size.x / 2) randomPoint.x = -chunk.Size.x / 2;

            if (randomPoint.y > chunk.Size.y / 2) randomPoint.y = chunk.Size.y / 2;
            if (randomPoint.y < -chunk.Size.y / 2) randomPoint.y = -chunk.Size.y / 2;

            return randomPoint + chunk.transform.position.XY();
        }

        public Vector2 GetBossSpawnPosition(BossFenceBehavior fence, Vector2 offset)
        {
            if (fence is CircleFenceBehavior circleFence)
            {
                float diagonal = Mathf.Sqrt(Mathf.Pow(chunk.Size.x / 2, 2) + Mathf.Pow(chunk.Size.y / 2, 2)) * 1.1f;
                circleFence.SetRadiusOverride(diagonal);
            }
            else if (fence is RectFenceBehavior rectFence)
            {
                rectFence.SetSizeOverride(chunk.Size.x * 1.1f, chunk.Size.y * 1.1f);
            }

            return Vector2.zero;
        }

        public bool IsPointOutsideRight(Vector2 point, out float distance)
        {
            bool result = point.x > chunk.RightBound;
            distance = result ? point.x - chunk.RightBound : 0;
            return result;
        }

        public bool IsPointOutsideLeft(Vector2 point, out float distance)
        {
            bool result = point.x < chunk.LeftBound;
            distance = result ? chunk.LeftBound - point.x : 0;
            return result;
        }

        public bool IsPointOutsideTop(Vector2 point, out float distance)
        {
            bool result = point.y > chunk.TopBound;
            distance = result ? point.y - chunk.TopBound : 0;
            return result;
        }

        public bool IsPointOutsideBottom(Vector2 point, out float distance)
        {
            bool result = point.y < chunk.BottomBound;
            distance = result ? chunk.BottomBound - point.y : 0;
            return result;
        }

        public void Clear()
        {
            Object.Destroy(chunk.gameObject);

            for(int i = 0; i < borders.Count; i++)
            {
                Object.Destroy(borders[i].gameObject);
            }

            borders.Clear();
        }
    }
}