using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using OctoberStudio.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio
{
    public class HorizontalFieldBehavior : IFieldBehavior
    {
        private PoolComponent<StageChunkBehavior> backgroundPool;
        private PoolComponent<Transform> topPool;
        private PoolComponent<Transform> bottomPool;

        private List<StageChunkBehavior> chunks = new List<StageChunkBehavior>();

        bool wait = false;

        public void Init(StageFieldData stageFieldData)
        {
            backgroundPool = new PoolComponent<StageChunkBehavior>("Field Background", stageFieldData.BackgroundPrefab, 4);

            if (stageFieldData.LeftPrefab != null)
            {
                topPool = new PoolComponent<Transform>("Field Top Border", stageFieldData.TopPrefab, 4);
            }

            if (stageFieldData.RightPrefab != null)
            {
                bottomPool = new PoolComponent<Transform>("Field Bottom Border", stageFieldData.BottomPrefab, 4);
            }

            var chunk = backgroundPool.GetEntity();

            chunk.transform.position = Vector3.zero;
            chunk.transform.rotation = Quaternion.identity;
            chunk.transform.localScale = Vector3.one;

            AddBordersToChunk(chunk);

            chunks.Add(chunk);

            wait = false;
            EasingManager.DoNextFrame().SetOnFinish(() => wait = true);
        }

        public void Update()
        {
            if (!wait) return;

            RemoveInvisibleChunks();
            CheckForNewChunks();
        }

        #region Add New Chunks

        private void CheckForNewChunks()
        {
            TryAddRightRow();
            TryAddLeftRow();
        }

        private void TryAddRightRow()
        {
            if (chunks[0].HasEmptyRight)
            {
                var chunk = backgroundPool.GetEntity();
                var chunkBellow = chunks[0];

                chunk.transform.position = chunkBellow.transform.position + Vector3.right * chunk.Size.y;
                chunk.transform.rotation = Quaternion.identity;
                chunk.transform.localScale = Vector3.one;

                AddBordersToChunk(chunk);

                chunks.Insert(0, chunk);
            }
        }

        private void TryAddLeftRow()
        {
            if (chunks[^1].HasEmptyLeft)
            {
                var chunk = backgroundPool.GetEntity();
                var chunkOnTop = chunks[^1];

                chunk.transform.position = chunkOnTop.transform.position + Vector3.left * chunk.Size.y;
                chunk.transform.rotation = Quaternion.identity;
                chunk.transform.localScale = Vector3.one;

                AddBordersToChunk(chunk);

                chunks.Add(chunk);
            }
        }

        private void AddBordersToChunk(StageChunkBehavior chunk)
        {
            if (topPool != null)
            {
                var topBorder = topPool.GetEntity();
                topBorder.transform.position = chunk.transform.position + Vector3.up * chunk.Size.y;

                chunk.AddBorder(topBorder);
            }

            if (bottomPool != null)
            {
                var bottomBorder = bottomPool.GetEntity();
                bottomBorder.transform.position = chunk.transform.position + Vector3.down * chunk.Size.y;

                chunk.AddBorder(bottomBorder);
            }
        }

        #endregion

        #region Remove Invisible Chunks

        private void RemoveInvisibleChunks()
        {
            if (chunks.Count == 0) return;

            CheckRightRow();
            CheckLeftRow();
        }

        private void CheckRightRow()
        {
            if (!chunks[0].IsVisible)
            {
                RemoveRightRow();
            }
        }

        private void CheckLeftRow()
        {
            if (!chunks[^1].IsVisible)
            {
                RemoveLeftRow();
            }
        }

        private void RemoveRightRow()
        {
            chunks[0].Clear();
            chunks.RemoveAt(0);
        }

        private void RemoveLeftRow()
        {
            chunks[^1].Clear();
            chunks.RemoveAt(chunks.Count - 1);
        }

        #endregion


        public bool ValidatePosition(Vector2 position)
        {
            if (position.y > chunks[0].transform.position.y + chunks[0].Size.y / 2) return false;
            if (position.y < chunks[0].transform.position.y - chunks[0].Size.y / 2) return false;

            return true;
        }

        public Vector2 GetBossSpawnPosition(BossFenceBehavior fence, Vector2 offset)
        {
            var playerPosition = PlayerBehavior.Player.transform.position.XY();

            if (fence is CircleFenceBehavior circleFence)
            {
                if(circleFence.Radius < chunks[0].Size.y / 2f)
                {
                    circleFence.SetRadiusOverride(chunks[0].Size.y / 2f * 1.1f);
                }

                var positionWithOffset = new Vector2(playerPosition.x, offset.y);
                if(Vector3.Distance(positionWithOffset, playerPosition) < circleFence.Radius)
                {
                    return positionWithOffset;
                } else
                {
                    return new Vector2(playerPosition.x, 0);
                }
            }
            else if(fence is RectFenceBehavior rectFence)
            {
                if(rectFence.Height < chunks[0].Size.y)
                {
                    rectFence.SetSizeOverride(rectFence.Width, chunks[0].Size.x * 1.1f);
                }

                return new Vector2(playerPosition.x, 0);
            }

            return playerPosition + offset;
        }

        public Vector2 GetRandomPositionOnBorder()
        {
            float y = Random.Range(-chunks[0].Size.y / 2, chunks[0].Size.y / 2) + chunks[0].transform.position.y;

            float sign = Random.Range(0, 2) * 2 - 1;
            float x = PlayerBehavior.Player.transform.position.x + CameraManager.HalfWidth * 1.05f * sign;

            return new Vector2(x, y);
        }

        public bool IsPointOutsideRight(Vector2 point, out float distance)
        {
            distance = 0;
            return false;
        }

        public bool IsPointOutsideLeft(Vector2 point, out float distance)
        {
            distance = 0;
            return false;
        }

        public bool IsPointOutsideTop(Vector2 point, out float distance)
        {
            bool result = point.y > chunks[0].TopBound;
            distance = result ? point.y - chunks[0].TopBound : 0;
            return result;
        }

        public bool IsPointOutsideBottom(Vector2 point, out float distance)
        {
            bool result = point.y < chunks[0].BottomBound;
            distance = result ? chunks[0].BottomBound - point.y : 0;
            return result;
        }

        public void Clear()
        {
            backgroundPool.Destroy();
        }
    }
}